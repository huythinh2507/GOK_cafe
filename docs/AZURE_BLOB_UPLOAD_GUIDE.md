# Azure Blob Storage Upload - Integration Guide

## Overview
This guide explains how to use the Azure Blob Storage upload functionality for the GOKCafe Create Product admin page.

## Architecture

### Components
1. **MediaController** - API endpoint for uploading files (`/api/v1/media/upload`)
2. **IAzureBlobService** - Service interface for Azure Blob operations
3. **AzureBlobService** - Implementation of Azure Blob Storage upload
4. **UploadFileResultDto** - Data transfer object for upload results

### File Locations
- Controller: `GOKCafe.API/Controllers/MediaController.cs`
- Service Interface: `GOKCafe.Infrastructure/Services/Interfaces/IAzureBlobService.cs`
- Service Implementation: `GOKCafe.Infrastructure/Services/AzureBlobService.cs`
- DTO: `GOKCafe.Infrastructure/DTOs/UploadFileResultDto.cs`

## Configuration

### appsettings.json
Add the following configuration to your `appsettings.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=mentorhubstorage;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "mentorhub",
    "AccountName": "mentorhubstorage",
    "BaseImgUrl": "https://mentorhubstorage.blob.core.windows.net/mentorhub/media/image/",
    "BaseUrl": "https://mentorhubstorage.blob.core.windows.net/mentorhub/",
    "MapImagesFolder": "event-maps"
  }
}
```

### Service Registration
The service is already registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
```

## API Usage

### Upload Endpoint

**URL:** `POST /api/v1/media/upload`

**Authentication:** Anonymous (AllowAnonymous)

**Request:**
- Content-Type: multipart/form-data
- Body: Form data with a file field

**Example using JavaScript:**
```javascript
const formData = new FormData();
formData.append('file', fileObject);

const response = await fetch(`${API_BASE_URL}/api/v1/media/upload`, {
    method: 'POST',
    body: formData
});

const result = await response.json();
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "fileUrl": "https://mentorhubstorage.blob.core.windows.net/mentorhub/media/image/media_guid.jpg",
    "fileName": "media_guid.jpg",
    "fileType": "image/jpeg",
    "fileSize": 123.45
  }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "File size exceeds the maximum limit of 15MB."
}
```

### Validation Rules
- **Allowed file types:**
  - Images: jpeg, jpg, png, gif, webp
  - Videos: mp4, mpeg
- **Maximum file size:** 15MB
- **File naming:** Files are renamed to `media_{guid}{extension}`
- **Storage location:**
  - Images: `media/image/` folder
  - Videos: `media/video/` folder

### Delete Endpoint

**URL:** `DELETE /api/v1/media`

**Authentication:** Required (Authorize attribute)

**Request:**
- Query parameter: `fileUrl` - The URL of the file to delete

**Example:**
```javascript
const response = await fetch(`${API_BASE_URL}/api/v1/media?fileUrl=${encodeURIComponent(fileUrl)}`, {
    method: 'DELETE',
    headers: {
        'Authorization': 'Bearer YOUR_TOKEN'
    }
});
```

## Frontend Integration (Create Product Page)

The upload functionality is already integrated into the Create Product page at:
`GOKCafe.Web/Views/ProductsAdmin/Create.cshtml`

### Key Features:
1. **Drag and drop** upload support
2. **Image cropping** using Cropper.js
3. **Multiple image upload** with preview
4. **Default image selection**
5. **Upload progress** indication
6. **Error handling** with user feedback

### Configuration:
Set the API base URL in the view:
```javascript
window.API_BASE_URL = 'https://localhost:7045';
```

### Upload Flow:
1. User selects/drops image file
2. Crop modal opens for image editing
3. Cropped image is uploaded to Azure Blob Storage via API
4. API returns the uploaded file URL
5. URL is stored in the product form for submission

## Service Methods

### IAzureBlobService Interface

```csharp
// Upload a media file
Task<UploadFileResultDto> UploadMediaFileAsync(IFormFile file);

// Upload a stream with automatic folder detection
Task<string> UploadMediaAsync(Stream stream, string fileName, string contentType);

// Upload a stream to specific location
Task<string> UploadAsync(Stream stream, string fileName, string contentType);

// Delete a file
Task<bool> DeleteFileAsync(string fileUrl);
```

## Troubleshooting

### Common Issues

1. **Build Error: Missing Package Reference**
   - Ensure `Azure.Storage.Blobs` package (v12.22.2) is installed in Infrastructure project
   - Run: `dotnet restore`

2. **Connection String Not Found**
   - Verify `AzureStorage:ConnectionString` is set in appsettings.json
   - Check that the configuration section is properly loaded

3. **Upload Fails with 500 Error**
   - Check Azure Storage account credentials
   - Verify container exists and has public blob access
   - Review application logs for detailed error messages

4. **CORS Issues**
   - Ensure CORS is properly configured in Program.cs
   - Verify Azure Storage container allows CORS from your domain

## Security Considerations

1. **Connection String Security**
   - Never commit real connection strings to source control
   - Use Azure Key Vault or environment variables for production
   - Use User Secrets for local development

2. **File Validation**
   - Always validate file types on both client and server
   - Enforce file size limits
   - Consider scanning files for malware in production

3. **Authentication**
   - Upload endpoint is currently anonymous for development
   - Consider adding authentication for production use

## Testing

### Manual Testing
1. Navigate to `/admin/products/create`
2. Upload an image using drag-and-drop or file selector
3. Crop the image
4. Verify the image uploads successfully
5. Check that the URL appears in the form

### Verify Azure Storage
1. Log in to Azure Portal
2. Navigate to your Storage Account
3. Open Blob containers
4. Check `mentorhub/media/image/` for uploaded files

## Next Steps

1. **Production Deployment:**
   - Update connection string in production appsettings
   - Configure Azure CDN for better performance
   - Enable Azure Storage analytics for monitoring

2. **Enhancements:**
   - Add image optimization/compression
   - Implement multiple image upload for product galleries
   - Add thumbnail generation
   - Implement progress tracking for large files

## Support

For issues or questions, please contact the development team or create an issue in the project repository.
