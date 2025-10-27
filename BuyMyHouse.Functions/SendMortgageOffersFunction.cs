using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuyMyHouse.Functions;

/// <summary>
/// Azure Function 2: Timer-triggered email notification sender
/// Runs in the morning to send mortgage offer emails to customers
/// </summary>
public class SendMortgageOffersFunction
{
    private readonly ILogger<SendMortgageOffersFunction> _logger;
    private readonly IMortgageRepository _mortgageRepository;
    private readonly INotificationService _notificationService;

    public SendMortgageOffersFunction(
        ILogger<SendMortgageOffersFunction> logger,
        IMortgageRepository mortgageRepository,
        INotificationService notificationService)
    {
        _logger = logger;
        _mortgageRepository = mortgageRepository;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Runs daily at 9:00 AM to send mortgage offer emails
    /// CRON format: "0 0 9 * * *" = At 09:00:00 every day
    /// For testing: "0 */10 * * * *" = Every 10 minutes
    /// </summary>
    [Function("SendMortgageOffers")]
    public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("SendMortgageOffers function started at: {Time}", DateTime.UtcNow);

        try
        {
            // Get all approved applications that haven't been sent yet (CQRS Read)
            var approvedApplications = await GetApprovedApplicationsAsync();
            var applicationsList = approvedApplications.ToList();

            _logger.LogInformation("Found {Count} approved mortgage offers to send", applicationsList.Count);

            var sentCount = 0;

            foreach (var application in applicationsList)
            {
                try
                {
                    _logger.LogInformation(
                        "Sending mortgage offer email for application {ApplicationId} to {ApplicantEmail}",
                        application.Id, application.ApplicantEmail);

                    // Generate offer URL (time-limited access)
                    var offerUrl = GenerateOfferUrl(application.Id);

                    // Send email notification
                    await _notificationService.SendOfferEmailAsync(
                        application.ApplicantEmail,
                        application.ApplicantName,
                        offerUrl);

                    // Update status to OfferSent (CQRS Write)
                    await _mortgageRepository.UpdateApplicationStatusAsync(
                        application.Id, 
                        MortgageStatus.OfferSent);

                    _logger.LogInformation(
                        "Successfully sent mortgage offer for application {ApplicationId}",
                        application.Id);

                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error sending mortgage offer for application {ApplicationId}", 
                        application.Id);
                }
            }

            _logger.LogInformation(
                "SendMortgageOffers completed - Sent: {Sent} emails",
                sentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMortgageOffers function");
        }

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {NextSchedule}", timerInfo.ScheduleStatus.Next);
        }
    }

    private async Task<IEnumerable<MortgageApplication>> GetApprovedApplicationsAsync()
    {
        _logger.LogInformation("Querying for approved applications that need offer emails sent");
        
        // Query for applications with status "Approved"
        return await _mortgageRepository.GetApprovedApplicationsAsync();
    }

    private string GenerateOfferUrl(int applicationId)
    {
        // Generate a time-limited URL for the mortgage offer
        // In production, this would be a secured endpoint with token
        var baseUrl = "https://buymyhouse.com/mortgage-offers";
        var token = Guid.NewGuid().ToString("N");
        
        return $"{baseUrl}/{applicationId}?token={token}&expires=7d";
    }
}
