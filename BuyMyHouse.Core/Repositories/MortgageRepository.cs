using System.Globalization;
using Azure;
using Azure.Data.Tables;
using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Core.Repositories;

public class MortgageRepository : IMortgageRepository
{
    private readonly TableClient _applicationsTableClient;
    private readonly TableClient _incomeTableClient;
    private readonly TableClient _counterTableClient;

    public MortgageRepository(string connectionString)
    {
        _applicationsTableClient = new TableClient(connectionString, "MortgageApplications");
        _incomeTableClient = new TableClient(connectionString, "ApplicantIncomes");
        _counterTableClient = new TableClient(connectionString, "Counters");

        _applicationsTableClient.CreateIfNotExists();
        _incomeTableClient.CreateIfNotExists();
        _counterTableClient.CreateIfNotExists();
    }

    private async Task<int> RetrieveNextIdentifierAsync()
    {
        const string counterKey = "MortgageApplicationCounter";
        try
        {
            var entity = await _counterTableClient.GetEntityAsync<TableEntity>("Counter", counterKey);
            var currentId = entity.Value.GetInt32("CurrentId") ?? 0;
            var nextId = currentId + 1;
            
            entity.Value["CurrentId"] = nextId;
            await _counterTableClient.UpdateEntityAsync(entity.Value, ETag.All);
            
            return nextId;
        }
        catch (Azure.RequestFailedException)
        {
            // Initialize counter if it doesn't exist yet
            var entity = new TableEntity("Counter", counterKey)
            {
                { "CurrentId", 1 }
            };
            await _counterTableClient.AddEntityAsync(entity);
            return 1;
        }
    }

    public async Task SaveNewApplicationAsync(MortgageApplication application)
    {
        // Auto-assign next available ID for this application
        application.Id = await RetrieveNextIdentifierAsync();
        
        var entity = new TableEntity(application.CurrentStatus.ToString(), application.Id.ToString())
        {
            { "ApplicantEmail", application.CandidateEmail },
            { "ApplicantName", application.CandidateName },
            { "AnnualIncome", (double)application.YearlyIncome },
            { "RequestedAmount", (double)application.LoanAmount },
            { "HouseId", application.PropertyId },
            { "ApplicationDate", application.SubmittedDate },
            { "Status", application.CurrentStatus.ToString() }
        };

        await _applicationsTableClient.AddEntityAsync(entity);
    }

    public async Task ChangeApplicationStateAsync(int applicationId, ApplicationState status)
    {
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>())
        {
            if (entity.RowKey == applicationId.ToString())
            {
                // Relocate entity to appropriate partition based on updated status
                await _applicationsTableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
                
                entity.PartitionKey = status.ToString();
                entity["Status"] = status.ToString();
                
                await _applicationsTableClient.AddEntityAsync(entity);
                break;
            }
        }
    }
    public async Task<MortgageApplication?> FetchApplicationByIdAsync(int applicationId)
    {
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(e => e.RowKey == applicationId.ToString()))
        {
            return MapToApplication(entity);
        }
        return null;
    }
    public async Task<IEnumerable<MortgageApplication>> FetchAwaitingApplicationsAsync()
    {
        var applications = new List<MortgageApplication>();
        
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(
            e => e.PartitionKey == ApplicationState.AwaitingReview.ToString()))
        {
            applications.Add(MapToApplication(entity));
        }
        
        return applications;
    }

    public async Task<IEnumerable<MortgageApplication>> FetchAcceptedApplicationsAsync()
    {
        var applications = new List<MortgageApplication>();
        
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(
            e => e.PartitionKey == ApplicationState.Accepted.ToString()))
        {
            applications.Add(MapToApplication(entity));
        }
        
        return applications;
    }

    public async Task StoreCandidateIncomeAsync(ApplicantIncome income)
    {
        var entity = new TableEntity(income.PartitionKey, income.RowKey)
        {
            { "ApplicantEmail", income.CandidateEmailAddress },
            { "AnnualIncome", income.YearlyIncome },
            { "RecordedDate", income.CreationTimestamp },
            { "ApplicationId", income.ApplicationIdentifier }
        };

        await _incomeTableClient.UpsertEntityAsync(entity);
    }

    private static MortgageApplication MapToApplication(TableEntity entity)
    {
        return new MortgageApplication
        {
            Id = Convert.ToInt32(entity.RowKey),
            CandidateEmail = entity.GetString("ApplicantEmail") ?? string.Empty,
            CandidateName = entity.GetString("ApplicantName") ?? string.Empty,
            YearlyIncome = entity.ContainsKey("AnnualIncome") && entity["AnnualIncome"] != null 
                ? Convert.ToDecimal(entity["AnnualIncome"]) 
                : 0,
            LoanAmount = entity.ContainsKey("RequestedAmount") && entity["RequestedAmount"] != null 
                ? Convert.ToDecimal(entity["RequestedAmount"]) 
                : 0,
            PropertyId = entity.ContainsKey("HouseId") && entity["HouseId"] != null 
                ? Convert.ToInt32(entity["HouseId"]) 
                : 0,
            SubmittedDate = entity.GetDateTime("ApplicationDate") ?? DateTime.UtcNow,
            CurrentStatus = Enum.Parse<ApplicationState>(entity.GetString("Status") ?? "AwaitingReview")
        };
    }

}