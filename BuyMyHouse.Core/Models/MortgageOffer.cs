namespace BuyMyHouse.Core.Models;

public class MortgageOffer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int ApplicationId { get; set; }

    public decimal ApprovedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermInYears { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime OfferDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string OfferDocumentUrl { get; set; } = string.Empty;
}