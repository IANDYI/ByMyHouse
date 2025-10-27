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

    private async Task<int> GetNextIdAsync()
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
            // Counter doesn't exist, create it
            var entity = new TableEntity("Counter", counterKey)
            {
                { "CurrentId", 1 }
            };
            await _counterTableClient.AddEntityAsync(entity);
            return 1;
        }
    }

    public async Task CreateApplicationAsync(MortgageApplication application)
    {
        // Generate auto-increment ID
        application.Id = await GetNextIdAsync();
        
        var entity = new TableEntity(application.Status.ToString(), application.Id.ToString())
        {
            { "ApplicantEmail", application.ApplicantEmail },
            { "ApplicantName", application.ApplicantName },
            { "AnnualIncome", (double)application.AnnualIncome },
            { "RequestedAmount", (double)application.RequestedAmount },
            { "HouseId", application.HouseId },
            { "ApplicationDate", application.ApplicationDate },
            { "Status", application.Status.ToString() }
        };

        await _applicationsTableClient.AddEntityAsync(entity);
    }

    public async Task UpdateApplicationStatusAsync(int applicationId, MortgageStatus status)
    {
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>())
        {
            if (entity.RowKey == applicationId.ToString())
            {
                // Need to move entity to new partition (status changed)
                await _applicationsTableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
                
                entity.PartitionKey = status.ToString();
                entity["Status"] = status.ToString();
                
                await _applicationsTableClient.AddEntityAsync(entity);
                break;
            }
        }
    }
    public async Task<MortgageApplication?> GetApplicationByIdAsync(int applicationId)
    {
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(e => e.RowKey == applicationId.ToString()))
        {
            return MapToApplication(entity);
        }
        return null;
    }
    public async Task<IEnumerable<MortgageApplication>> GetPendingApplicationsAsync()
    {
        var applications = new List<MortgageApplication>();
        
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(
            e => e.PartitionKey == MortgageStatus.Pending.ToString()))
        {
            applications.Add(MapToApplication(entity));
        }
        
        return applications;
    }

    public async Task<IEnumerable<MortgageApplication>> GetApprovedApplicationsAsync()
    {
        var applications = new List<MortgageApplication>();
        
        await foreach (var entity in _applicationsTableClient.QueryAsync<TableEntity>(
            e => e.PartitionKey == MortgageStatus.Approved.ToString()))
        {
            applications.Add(MapToApplication(entity));
        }
        
        return applications;
    }

    public async Task SaveApplicantIncomeAsync(ApplicantIncome income)
    {
        var entity = new TableEntity(income.PartitionKey, income.RowKey)
        {
            { "ApplicantEmail", income.ApplicantEmail },
            { "AnnualIncome", income.AnnualIncome },
            { "RecordedDate", income.RecordedDate },
            { "ApplicationId", income.ApplicationId }
        };

        await _incomeTableClient.UpsertEntityAsync(entity);
    }

    private static MortgageApplication MapToApplication(TableEntity entity)
    {
        return new MortgageApplication
        {
            Id = Convert.ToInt32(entity.RowKey),
            ApplicantEmail = entity.GetString("ApplicantEmail") ?? string.Empty,
            ApplicantName = entity.GetString("ApplicantName") ?? string.Empty,
            AnnualIncome = entity.ContainsKey("AnnualIncome") && entity["AnnualIncome"] != null 
                ? Convert.ToDecimal(entity["AnnualIncome"]) 
                : 0,
            RequestedAmount = entity.ContainsKey("RequestedAmount") && entity["RequestedAmount"] != null 
                ? Convert.ToDecimal(entity["RequestedAmount"]) 
                : 0,
            HouseId = entity.ContainsKey("HouseId") && entity["HouseId"] != null 
                ? Convert.ToInt32(entity["HouseId"]) 
                : 0,
            ApplicationDate = entity.GetDateTime("ApplicationDate") ?? DateTime.UtcNow,
            Status = Enum.Parse<MortgageStatus>(entity.GetString("Status") ?? "Pending")
        };
    }

}