namespace BuyMyHouse.Core.Models;

public class House
{
    public int Id { get; set; }
    public string PropertyAddress { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;
    public decimal ListingPrice { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public int AreaInSquareMeters { get; set; }
    public string PropertyDescription { get; set; } = string.Empty;
    public List<string> PictureUrls { get; set; } = new();
    public DateTime DateListed { get; set; }
    public bool CurrentlyAvailable { get; set; }
}