using GOKCafe.Infrastructure.DTOs;
using Microsoft.AspNetCore.Http;

namespace GOKCafe.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for handling Azure Blob Storage operations
/// </summary>
public interface IAzureBlobService
{
    /// <summary>
    /// Upload a media file (image/video) to Azure Blob Storage
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Upload result with file URL and metadata</returns>
    Task<UploadFileResultDto> UploadMediaFileAsync(IFormFile file);

    /// <summary>
    /// Upload a stream to Azure Blob Storage
    /// </summary>
    /// <param name="stream">The stream to upload</param>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The content type</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);

    /// <summary>
    /// Upload a media stream to Azure Blob Storage (images or videos)
    /// </summary>
    /// <param name="stream">The stream to upload</param>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The content type</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadMediaAsync(Stream stream, string fileName, string contentType);

    /// <summary>
    /// Delete a file from Azure Blob Storage
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteFileAsync(string fileUrl);
}
