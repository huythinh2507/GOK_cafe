namespace GOKCafe.Infrastructure.DTOs;

/// <summary>
/// DTO for file upload result
/// </summary>
public class UploadFileResultDto
{
    /// <summary>
    /// The name of the uploaded file
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The URL/path of the uploaded file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The size of the file in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// The content type of the file
    /// </summary>
    public string FileType { get; set; } = string.Empty;
}
