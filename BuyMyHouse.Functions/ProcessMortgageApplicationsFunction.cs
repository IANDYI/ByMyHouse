using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuyMyHouse.Functions;

/// <summary>
/// Scheduled batch processor triggered by timer
/// Executes nightly to evaluate and process mortgage applications in queue
/// </summary>
public class ProcessMortgageApplicationsFunction
{
    private readonly ILogger<ProcessMortgageApplicationsFunction> _logger;
    private readonly IMortgageRepository _mortgageRepository;
    private readonly IMortgageOfferService _mortgageOfferService;
    private readonly INotificationService _notificationService;

    public ProcessMortgageApplicationsFunction(
        ILogger<ProcessMortgageApplicationsFunction> logger,
        IMortgageRepository mortgageRepository,
        IMortgageOfferService mortgageOfferService,
        INotificationService notificationService)
    {
        _logger = logger;
        _mortgageRepository = mortgageRepository;
        _mortgageOfferService = mortgageOfferService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Scheduled execution daily at 11:00 PM for batch processing
    /// CRON expression: "0 0 23 * * *" executes at 23:00:00 daily
    /// Test mode CRON: "0 */5 * * * *" runs every 5 minutes
    /// </summary>
    [Function("ProcessMortgageApplications")]
    public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("ProcessMortgageApplications function started at: {Time}", DateTime.UtcNow);

        try
        {
            // Query all applications awaiting processing
            var pendingApplications = await _mortgageRepository.FetchAwaitingApplicationsAsync();
            var applicationsList = pendingApplications.ToList();

            _logger.LogInformation("Found {Count} pending mortgage applications to process", applicationsList.Count);

            var processedCount = 0;
            var approvedCount = 0;
            var rejectedCount = 0;

            foreach (var application in applicationsList)
            {
                try
                {
                    _logger.LogInformation("Processing mortgage application {ApplicationId} for {ApplicantEmail}", 
                        application.Id, application.CandidateEmail);

                    // Transition application to processing state
                    await _mortgageRepository.ChangeApplicationStateAsync(application.Id, ApplicationState.UnderProcessing);

                    // Check if application is approved
                    var isApproved = _mortgageOfferService.IsApproved(application);

                    if (isApproved)
                    {
                        // Create mortgage proposal for approved application
                        var offer = await _mortgageOfferService.GenerateOfferAsync(application);

                        // Create and persist offer document to blob storage
                        var documentUrl = await _notificationService.GenerateOfferDocumentAsync(
                            application.Id,
                            offer.LoanOfferAmount,
                            offer.AnnualInterestRate,
                            offer.DurationYears,
                            offer.PaymentMonthly);

                        offer.DocumentLink = documentUrl;

                        // Mark application as accepted
                        await _mortgageRepository.ChangeApplicationStateAsync(application.Id, ApplicationState.Accepted);

                        // Notification email will be dispatched next morning by separate function
                        // Logging offer readiness for audit purposes
                        _logger.LogInformation(
                            "Mortgage application {ApplicationId} approved - Offer document: {DocumentUrl}",
                            application.Id, documentUrl);

                        approvedCount++;
                    }
                    else
                    {
                        // Mark application as declined
                        await _mortgageRepository.ChangeApplicationStateAsync(application.Id, ApplicationState.Declined);

                        _logger.LogInformation("Mortgage application {ApplicationId} rejected", application.Id);

                        rejectedCount++;
                    }

                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing mortgage application {ApplicationId}", application.Id);
                }
            }

            _logger.LogInformation(
                "ProcessMortgageApplications completed - Processed: {Processed}, Approved: {Approved}, Rejected: {Rejected}",
                processedCount, approvedCount, rejectedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessMortgageApplications function");
        }

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {NextSchedule}", timerInfo.ScheduleStatus.Next);
        }
    }
}
