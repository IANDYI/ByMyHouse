using System.ComponentModel.DataAnnotations;

namespace BuyMyHouse.Mortgage.Models;

public class CreateMortgageApplicationDto
{
    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string CandidateName { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal YearlyIncome { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal LoanAmount { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }
}
