using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuyMyHouse.Core.Interfaces;

namespace BuyMyHouse.Core.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _containerClient;

    public ImageStorageService()
    {
        // Local Azurite connection string for development environment
        var connectionString = "UseDevelopmentStorage=true";
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient("house-images");
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> StorePropertyPictureAsync(int houseId, Stream imageStream, string fileName)
    {
        // Build unique blob path structure: house-{id}/{timestamp}-{filename}
        var blobName = $"house-{houseId}/{DateTimeOffset.UtcNow.Ticks}-{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Persist image data to blob storage
        await blobClient.UploadAsync(imageStream, overwrite: true);

        // Provide blob URL for retrieval
        return blobClient.Uri.ToString();
    }

    public async Task<List<string>> FetchPropertyPictureUrlsAsync(int houseId)
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

    public async Task RemovePropertyPictureAsync(string imageUrl)
    {
        var uri = new Uri(imageUrl);
        var blobName = uri.Segments[^1]; // Extract final path segment
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
