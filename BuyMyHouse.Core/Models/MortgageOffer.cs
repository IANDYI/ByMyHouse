namespace BuyMyHouse.Core.Models;

public class MortgageOffer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int ApplicationId { get; set; }

    public decimal LoanOfferAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int DurationYears { get; set; }
    public decimal PaymentMonthly { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ValidUntilDate { get; set; }
    public string DocumentLink { get; set; } = string.Empty;
}