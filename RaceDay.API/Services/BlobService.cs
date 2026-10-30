using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services;

public class BlobService : IBlobService
{
    private readonly string _connectionString;
    private readonly string _containerName = "event-banners";

    public BlobService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureBlobStorage"]!;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        // Ensure container exists (it should, from Azure Portal)
        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // Create a unique filename so we don't overwrite images
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

        return blobClient.Uri.ToString();
    }
}