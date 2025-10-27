namespace BuyMyHouse.Core.Models;

// Table Storage entity for CQRS write side
public class ApplicantIncome
{
    public string PartitionKey { get; set; } = string.Empty; // Year-Month
    public string RowKey { get; set; } = string.Empty; // ApplicationId
    public string ApplicantEmail { get; set; } = string.Empty;
    public decimal AnnualIncome { get; set; }
    public DateTime RecordedDate { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
}