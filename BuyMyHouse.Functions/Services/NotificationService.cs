using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BuyMyHouse.Functions.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendOfferEmailAsync(string toEmail, string applicantName, string offerUrl)
    {
        // In a real application, you would use SendGrid or another email service
        // For local testing, we'll just log the email
        
        var emailBody = $@"
Dear {applicantName},

Your mortgage application has been processed!

Your mortgage offer is ready for review. Please click the link below to view your offer:
{offerUrl}

This offer is valid for 14 days from the date of generation.

If you have any questions, please don't hesitate to contact us.

Best regards,
BuyMyHouse Estate Agents
";

        _logger.LogInformation("Sending email to {Email}:\n{EmailBody}", toEmail, emailBody);
        await Task.CompletedTask;
    }

    public async Task<string> GenerateOfferDocumentAsync(
        int applicationId, 
        decimal approvedAmount, 
        decimal interestRate, 
        int termYears, 
        decimal monthlyPayment)
    {
        // Generate a simple text-based offer document
        var documentContent = $@"
MORTGAGE OFFER DOCUMENT
========================

Application ID: {applicationId}
Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

LOAN DETAILS
------------
Approved Amount: €{approvedAmount:N2}
Interest Rate: {interestRate}% per annum
Term: {termYears} years
Monthly Payment: €{monthlyPayment:N2}

TERMS AND CONDITIONS
--------------------
1. This offer is valid for 14 days from the date of generation.
2. The interest rate is fixed for the entire term.
3. Early repayment penalties may apply.
4. Property insurance is required.
5. This offer is subject to final approval and property valuation.

This is a binding offer upon acceptance.

BuyMyHouse Estate Agents
Regional Office
";

        try
        {
            // Store document in Azure Blob Storage (Azurite locally)
            var connectionString = _configuration["AzureStorageConnectionString"] ?? "UseDevelopmentStorage=true";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("mortgage-offers");
            
            await containerClient.CreateIfNotExistsAsync();

            var blobName = $"{applicationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            var blobClient = containerClient.GetBlobClient(blobName);

            var bytes = Encoding.UTF8.GetBytes(documentContent);
            using var stream = new MemoryStream(bytes);
            
            await blobClient.UploadAsync(stream, overwrite: true);

            var blobUrl = blobClient.Uri.ToString();
            _logger.LogInformation("Generated mortgage offer document: {BlobUrl}", blobUrl);

            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mortgage offer document for application {ApplicationId}", applicationId);
            return $"mock-url://{applicationId}.txt";
        }
    }
}
