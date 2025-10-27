using System.ComponentModel.DataAnnotations;

namespace BuyMyHouse.Mortgage.Models;

public class CreateMortgageApplicationDto
{
    [Required]
    [EmailAddress]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal AnnualIncome { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal RequestedAmount { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int HouseId { get; set; }
}
