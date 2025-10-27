namespace BuyMyHouse.Core.Models;

public class MortgageApplication
{
    public int Id { get; set; }
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public decimal AnnualIncome { get; set; }
    public decimal RequestedAmount { get; set; }
    public int HouseId { get; set; }
    public DateTime ApplicationDate { get; set; }

    public MortgageStatus Status { get; set; }
    public string? OfferDocumentUrl { get; set; }
}

public enum MortgageStatus
{
    Pending,
    Processing,
    Approved,
    Rejected,
    OfferSent
}