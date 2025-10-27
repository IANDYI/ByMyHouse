namespace BuyMyHouse.Core.Interfaces;

public interface IImageStorageService
{
    Task<string> StorePropertyPictureAsync(int houseId, Stream imageStream, string fileName);
    Task<List<string>> FetchPropertyPictureUrlsAsync(int houseId);
    Task RemovePropertyPictureAsync(string imageUrl);
}
