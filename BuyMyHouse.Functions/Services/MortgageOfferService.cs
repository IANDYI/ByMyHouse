using BuyMyHouse.Core.Models;
using Microsoft.Extensions.Logging;

namespace BuyMyHouse.Functions.Services;

public class MortgageOfferService : IMortgageOfferService
{
    private readonly ILogger<MortgageOfferService> _logger;

    public MortgageOfferService(ILogger<MortgageOfferService> logger)
    {
        _logger = logger;
    }

    public bool IsApproved(MortgageApplication application)
    {
        // Business rule: Maximum loan amount is 4.5x annual income
        var maxLoan = application.AnnualIncome * 4.5m;
        
        // Business rule: Minimum income threshold
        var minIncome = 25000m;

        var isApproved = application.AnnualIncome >= minIncome 
                        && application.RequestedAmount <= maxLoan;

        _logger.LogInformation(
            "Mortgage application {ApplicationId} - Requested: {Requested}, Max Allowed: {MaxLoan}, Approved: {IsApproved}",
            application.Id, application.RequestedAmount, maxLoan, isApproved);

        return isApproved;
    }

    public Task<MortgageOffer> GenerateOfferAsync(MortgageApplication application)
    {
        var maxLoan = application.AnnualIncome * 4.5m;
        var approvedAmount = Math.Min(application.RequestedAmount, maxLoan);

        // Calculate interest rate based on loan-to-income ratio
        var loanToIncomeRatio = approvedAmount / application.AnnualIncome;
        var interestRate = CalculateInterestRate(loanToIncomeRatio);

        // Standard mortgage term: 25 years
        var termInYears = 25;

        // Calculate monthly payment
        var monthlyPayment = CalculateMonthlyPayment(approvedAmount, interestRate, termInYears);

        var offer = new MortgageOffer
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationId = application.Id,
            ApprovedAmount = approvedAmount,
            InterestRate = interestRate,
            TermInYears = termInYears,
            MonthlyPayment = monthlyPayment,
            OfferDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(14), // Offer valid for 14 days
            OfferDocumentUrl = string.Empty // Will be set after document generation
        };

        _logger.LogInformation(
            "Generated mortgage offer {OfferId} for application {ApplicationId} - Amount: {Amount}, Rate: {Rate}%",
            offer.Id, application.Id, offer.ApprovedAmount, offer.InterestRate);

        return Task.FromResult(offer);
    }

    private static decimal CalculateInterestRate(decimal loanToIncomeRatio)
    {
        // Base interest rate: 3.0%
        var baseRate = 3.0m;

        // Add risk adjustment based on loan-to-income ratio
        if (loanToIncomeRatio > 4.0m)
            return baseRate + 2.5m; // 5.5%
        else if (loanToIncomeRatio > 3.5m)
            return baseRate + 2.0m; // 5.0%
        else if (loanToIncomeRatio > 3.0m)
            return baseRate + 1.5m; // 4.5%
        else
            return baseRate; // 3.0%
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInYears)
    {
        var monthlyRate = (double)(annualInterestRate / 100 / 12);
        var numberOfPayments = termInYears * 12;

        if (monthlyRate == 0)
            return principal / numberOfPayments;

        var monthlyPayment = (decimal)(
            (double)principal * 
            (monthlyRate * Math.Pow(1 + monthlyRate, numberOfPayments)) /
            (Math.Pow(1 + monthlyRate, numberOfPayments) - 1)
        );

        return Math.Round(monthlyPayment, 2);
    }
}
