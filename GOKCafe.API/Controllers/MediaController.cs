using GOKCafe.Infrastructure.DTOs;
using GOKCafe.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Controller for handling media file uploads
/// </summary>
[ApiController]
[Route("api/v1/media")]
public class MediaController : ControllerBase
{
    private readonly IAzureBlobService _blobService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        IAzureBlobService blobService,
        ILogger<MediaController> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a media file (image or video) to Azure Blob Storage
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Upload result with file URL and metadata</returns>
    [AllowAnonymous]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload attempt failed: empty file");
            return BadRequest(new
            {
                success = false,
                message = "No file uploaded."
            });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "video/mp4", "video/mpeg" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            _logger.LogWarning("Upload attempt failed: invalid file type {ContentType}", file.ContentType);
            return BadRequest(new
            {
                success = false,
                message = "Invalid file type. Only image and video files are allowed."
            });
        }

        // Validate file size (15MB max)
        const long maxFileSize = 15 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            _logger.LogWarning("Upload attempt failed: file too large {FileSize}", file.Length);
            return BadRequest(new
            {
                success = false,
                message = "File size exceeds the maximum limit of 15MB."
            });
        }

        try
        {
            _logger.LogInformation("Uploading file {FileName}, Size: {Size}", file.FileName, file.Length);

            var result = await _blobService.UploadMediaFileAsync(file);

            _logger.LogInformation("File uploaded successfully: {FileName}, URL: {FileUrl}",
                result.FileName, result.FilePath);

            return Ok(new
            {
                success = true,
                data = new
                {
                    fileUrl = result.FilePath,
                    fileName = result.FileName,
                    fileType = result.FileType,
                    fileSize = Math.Round((float)result.FileSize / 1024, 2) // Size in KB
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error while uploading file."
            });
        }
    }

    /// <summary>
    /// Delete a media file from Azure Blob Storage
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete</param>
    /// <returns>Success status</returns>
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteMedia([FromQuery] string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            return BadRequest(new
            {
                success = false,
                message = "File URL is required."
            });
        }

        try
        {
            var result = await _blobService.DeleteFileAsync(fileUrl);

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "File deleted successfully."
                });
            }
            else
            {
                return NotFound(new
                {
                    success = false,
                    message = "File not found."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error while deleting file."
            });
        }
    }
}
