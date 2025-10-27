using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuyMyHouse.Functions;

/// <summary>
/// Email dispatch function triggered on schedule
/// Executes in morning hours to deliver mortgage offers via email to applicants
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
    /// Scheduled to run daily at 9:00 AM for email delivery
    /// CRON pattern: "0 0 9 * * *" triggers at 09:00:00 daily
    /// Test environment: "0 */10 * * * *" executes every 10 minutes
    /// </summary>
    [Function("SendMortgageOffers")]
    public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("SendMortgageOffers function started at: {Time}", DateTime.UtcNow);

        try
        {
            // Retrieve approved applications requiring email delivery
            var approvedApplications = await FetchAcceptedApplications();
            var applicationsList = approvedApplications.ToList();

            _logger.LogInformation("Found {Count} approved mortgage offers to send", applicationsList.Count);

            var sentCount = 0;

            foreach (var application in applicationsList)
            {
                try
                {
                    _logger.LogInformation(
                        "Sending mortgage offer email for application {ApplicationId} to {ApplicantEmail}",
                        application.Id, application.CandidateEmail);

                    // Create time-restricted access URL for offer
                    var offerUrl = GenerateOfferUrl(application.Id);

                    // Dispatch email notification
                    await _notificationService.SendOfferEmailAsync(
                        application.CandidateEmail,
                        application.CandidateName,
                        offerUrl);

                    // Update status indicating offer has been delivered
                    await _mortgageRepository.ChangeApplicationStateAsync(
                        application.Id, 
                        ApplicationState.OfferDelivered);

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

    private async Task<IEnumerable<MortgageApplication>> FetchAcceptedApplications()
    {
        _logger.LogInformation("Querying for approved applications that need offer emails sent");
        
        // Filter applications that are in approved state
        return await _mortgageRepository.FetchAcceptedApplicationsAsync();
    }

    private string GenerateOfferUrl(int applicationId)
    {
        // Construct time-restricted URL for offer access
        // Production implementation would include authentication token
        var baseUrl = "https://buymyhouse.com/mortgage-offers";
        var token = Guid.NewGuid().ToString("N");
        
        return $"{baseUrl}/{applicationId}?token={token}&expires=7d";
    }
}
