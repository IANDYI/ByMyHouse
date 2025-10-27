namespace BuyMyHouse.Core.Models;

public class MortgageApplication
{
    public int Id { get; set; }
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public decimal YearlyIncome { get; set; }
    public decimal LoanAmount { get; set; }
    public int PropertyId { get; set; }
    public DateTime SubmittedDate { get; set; }

    public ApplicationState CurrentStatus { get; set; }
    public string? DocumentLinkUrl { get; set; }
}

public enum ApplicationState
{
    AwaitingReview,
    UnderProcessing,
    Accepted,
    Declined,
    OfferDelivered
}