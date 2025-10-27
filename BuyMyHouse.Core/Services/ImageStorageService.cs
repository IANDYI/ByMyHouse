using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuyMyHouse.Core.Interfaces;

namespace BuyMyHouse.Core.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _containerClient;

    public ImageStorageService()
    {
        // Use Azurite connection string for local development
        var connectionString = "UseDevelopmentStorage=true";
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient("house-images");
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadHouseImageAsync(int houseId, Stream imageStream, string fileName)
    {
        // Create a unique blob name: house-{id}/{timestamp}-{filename}
        var blobName = $"house-{houseId}/{DateTimeOffset.UtcNow.Ticks}-{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Upload the image
        await blobClient.UploadAsync(imageStream, overwrite: true);

        // Return the URL
        return blobClient.Uri.ToString();
    }

    public async Task<List<string>> GetHouseImageUrlsAsync(int houseId)
    {
        var imageUrls = new List<string>();
        var prefix = $"house-{houseId}/";

        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            var blobClient = _containerClient.GetBlobClient(blobItem.Name);
            imageUrls.Add(blobClient.Uri.ToString());
        }

        return imageUrls;
    }

    public async Task DeleteHouseImageAsync(string imageUrl)
    {
        var uri = new Uri(imageUrl);
        var blobName = uri.Segments[^1]; // Get last segment
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
