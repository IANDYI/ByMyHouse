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
        // Maximum allowed loan is 4.5 times yearly income
        var maxLoan = application.YearlyIncome * 4.5m;
        
        // Required minimum income level
        var minIncome = 25000m;

        var isApproved = application.YearlyIncome >= minIncome 
                        && application.LoanAmount <= maxLoan;

        _logger.LogInformation(
            "Mortgage application {ApplicationId} - Requested: {Requested}, Max Allowed: {MaxLoan}, Approved: {IsApproved}",
            application.Id, application.LoanAmount, maxLoan, isApproved);

        return isApproved;
    }

    public Task<MortgageOffer> GenerateOfferAsync(MortgageApplication application)
    {
        var maxLoan = application.YearlyIncome * 4.5m;
        var approvedAmount = Math.Min(application.LoanAmount, maxLoan);

        // Derive interest rate from loan-to-income ratio
        var loanToIncomeRatio = approvedAmount / application.YearlyIncome;
        var interestRate = CalculateInterestRate(loanToIncomeRatio);

        // Default mortgage duration: 25 years
        var termInYears = 25;

        // Compute monthly payment amount
        var monthlyPayment = CalculateMonthlyPayment(approvedAmount, interestRate, termInYears);

        var offer = new MortgageOffer
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationId = application.Id,
            LoanOfferAmount = approvedAmount,
            AnnualInterestRate = interestRate,
            DurationYears = termInYears,
            PaymentMonthly = monthlyPayment,
            CreatedDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(14), // Offer expires after 14 days
            DocumentLink = string.Empty // Populated after document creation
        };

        _logger.LogInformation(
            "Generated mortgage offer {OfferId} for application {ApplicationId} - Amount: {Amount}, Rate: {Rate}%",
            offer.Id, application.Id, offer.LoanOfferAmount, offer.AnnualInterestRate);

        return Task.FromResult(offer);
    }

    private static decimal CalculateInterestRate(decimal loanToIncomeRatio)
    {
        // Starting interest rate: 3.0%
        var baseRate = 3.0m;

        // Apply risk premium according to loan-to-income ratio
        if (loanToIncomeRatio > 4.0m)
            return baseRate + 2.5m; // High risk: 5.5%
        else if (loanToIncomeRatio > 3.5m)
            return baseRate + 2.0m; // Moderate-high: 5.0%
        else if (loanToIncomeRatio > 3.0m)
            return baseRate + 1.5m; // Moderate: 4.5%
        else
            return baseRate; // Low risk: 3.0%
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
