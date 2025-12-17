using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GOKCafe.Infrastructure.DTOs;
using GOKCafe.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GOKCafe.Infrastructure.Services;

/// <summary>
/// Service for handling Azure Blob Storage operations
/// </summary>
public class AzureBlobService : IAzureBlobService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobService> _logger;
    private readonly string _baseUrl;

    public AzureBlobService(
        IConfiguration configuration,
        ILogger<AzureBlobService> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:ContainerName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Storage connection string is not configured");
        }

        if (string.IsNullOrEmpty(containerName))
        {
            throw new InvalidOperationException("Azure Storage container name is not configured");
        }

        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);

        _baseUrl = configuration["AzureStorage:BaseUrl"] ?? _containerClient.Uri.ToString();

        _logger.LogInformation("Initialized AzureBlobService with container {ContainerName}", containerName);
    }

    /// <summary>
    /// Upload a media file (image/video) to Azure Blob Storage
    /// </summary>
    public async Task<UploadFileResultDto> UploadMediaFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("UploadMediaFileAsync called with empty file");
            throw new ArgumentException("No file uploaded");
        }

        try
        {
            string mediaId = Guid.NewGuid().ToString();
            string contentType = file.ContentType;
            string extension = Path.GetExtension(file.FileName);

            string fileName = $"media_{mediaId}{extension}";

            using var stream = file.OpenReadStream();
            string fileUrl = await UploadMediaAsync(stream, fileName, contentType);

            _logger.LogInformation("Media file uploaded successfully: {MediaId}, file {FileName}", mediaId, fileName);

            return new UploadFileResultDto
            {
                FileName = fileName,
                FilePath = fileUrl,
                FileSize = file.Length,
                FileType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media file {OriginalFileName}", file?.FileName);
            throw;
        }
    }

    /// <summary>
    /// Upload a media stream to Azure Blob Storage (images or videos)
    /// </summary>
    public async Task<string> UploadMediaAsync(Stream stream, string fileName, string contentType)
    {
        string folder = contentType.StartsWith("video/") ? "media/video" : "media/image";
        return await UploadAsync(stream, fileName, folder, contentType);
    }

    /// <summary>
    /// Upload a stream to Azure Blob Storage
    /// </summary>
    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        string folder = "uploads";
        return await UploadAsync(stream, fileName, folder, contentType);
    }

    /// <summary>
    /// Upload a stream to Azure Blob Storage with folder specification
    /// </summary>
    private async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, string contentType)
    {
        try
        {
            string blobName = $"{folder}/{fileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, options, cancellationToken: default);

            _logger.LogInformation("File {FileName} uploaded successfully to {BlobName}", fileName, blobName);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Delete a file from Azure Blob Storage
    /// </summary>
    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                _logger.LogWarning("DeleteFileAsync called with empty URL");
                return false;
            }

            // Extract blob name from URL
            var uri = new Uri(fileUrl);
            var blobName = uri.AbsolutePath.TrimStart('/');

            // Remove container name from blob name if present
            var containerName = _containerClient.Name;
            if (blobName.StartsWith(containerName + "/"))
            {
                blobName = blobName.Substring(containerName.Length + 1);
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            var result = await blobClient.DeleteIfExistsAsync();

            if (result.Value)
            {
                _logger.LogInformation("File deleted successfully: {BlobName}", blobName);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {BlobName}", blobName);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
            return false;
        }
    }
}
