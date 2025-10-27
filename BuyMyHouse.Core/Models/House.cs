namespace BuyMyHouse.Core.Models;

public class House
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int SquareMeters { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public DateTime ListedDate { get; set; }
    public bool IsAvailable { get; set; }
}