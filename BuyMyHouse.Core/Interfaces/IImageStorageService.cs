namespace BuyMyHouse.Core.Interfaces;

public interface IImageStorageService
{
    Task<string> UploadHouseImageAsync(int houseId, Stream imageStream, string fileName);
    Task<List<string>> GetHouseImageUrlsAsync(int houseId);
    Task DeleteHouseImageAsync(string imageUrl);
}
