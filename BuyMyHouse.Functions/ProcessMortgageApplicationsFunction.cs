using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BuyMyHouse.Functions;

/// <summary>
/// Azure Function 1: Timer-triggered batch processing
/// Runs at end of each day to process pending mortgage applications
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
    /// Runs daily at 11:00 PM (23:00) to process all pending applications
    /// CRON format: "0 0 23 * * *" = At 23:00:00 every day
    /// For testing: "0 */5 * * * *" = Every 5 minutes
    /// </summary>
    [Function("ProcessMortgageApplications")]
    public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("ProcessMortgageApplications function started at: {Time}", DateTime.UtcNow);

        try
        {
            // Get all pending applications (CQRS Read)
            var pendingApplications = await _mortgageRepository.GetPendingApplicationsAsync();
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
                        application.Id, application.ApplicantEmail);

                    // Update status to Processing (CQRS Write)
                    await _mortgageRepository.UpdateApplicationStatusAsync(application.Id, MortgageStatus.Processing);

                    // Check if application is approved
                    var isApproved = _mortgageOfferService.IsApproved(application);

                    if (isApproved)
                    {
                        // Generate mortgage offer
                        var offer = await _mortgageOfferService.GenerateOfferAsync(application);

                        // Generate offer document and store in blob storage
                        var documentUrl = await _notificationService.GenerateOfferDocumentAsync(
                            application.Id,
                            offer.ApprovedAmount,
                            offer.InterestRate,
                            offer.TermInYears,
                            offer.MonthlyPayment);

                        offer.OfferDocumentUrl = documentUrl;

                        // Update application status to Approved (CQRS Write)
                        await _mortgageRepository.UpdateApplicationStatusAsync(application.Id, MortgageStatus.Approved);

                        // Send notification email (will be sent in morning by another function)
                        // For now, we'll log that the offer is ready
                        _logger.LogInformation(
                            "Mortgage application {ApplicationId} approved - Offer document: {DocumentUrl}",
                            application.Id, documentUrl);

                        approvedCount++;
                    }
                    else
                    {
                        // Update status to Rejected (CQRS Write)
                        await _mortgageRepository.UpdateApplicationStatusAsync(application.Id, MortgageStatus.Rejected);

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
