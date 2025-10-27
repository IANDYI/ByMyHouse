namespace BuyMyHouse.Core.Models;

// Entity used for storing income data in Azure Table Storage following CQRS pattern
public class ApplicantIncome
{
    public string PartitionKey { get; set; } = string.Empty; // Format: YYYY-MM for monthly partitioning
    public string RowKey { get; set; } = string.Empty; // Unique application identifier
    public string CandidateEmailAddress { get; set; } = string.Empty;
    public decimal YearlyIncome { get; set; }
    public DateTime CreationTimestamp { get; set; }
    public string ApplicationIdentifier { get; set; } = string.Empty;
}