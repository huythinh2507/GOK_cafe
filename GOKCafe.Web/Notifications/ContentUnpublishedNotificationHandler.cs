using GOKCafe.Web.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace GOKCafe.Web.Notifications;

/// <summary>
/// Automatically delete blog from database when unpublished in Umbraco
/// </summary>
public class ContentUnpublishedNotificationHandler : INotificationHandler<ContentUnpublishedNotification>
{
    private readonly IUmbracoSyncService _umbracoSyncService;
    private readonly ILogger<ContentUnpublishedNotificationHandler> _logger;

    public ContentUnpublishedNotificationHandler(
        IUmbracoSyncService umbracoSyncService,
        ILogger<ContentUnpublishedNotificationHandler> logger)
    {
        _umbracoSyncService = umbracoSyncService;
        _logger = logger;
    }

    public void Handle(ContentUnpublishedNotification notification)
    {
        foreach (var entity in notification.UnpublishedEntities)
        {
            // Only process if it's a blog item
            if (entity.ContentType.Alias == "blogsItem")
            {
                try
                {
                    _logger.LogInformation($"Auto-deleting unpublished blog: {entity.Name} (ID: {entity.Id})");

                    // Run delete asynchronously without blocking Umbraco
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _umbracoSyncService.DeleteBlogByNodeIdAsync(entity.Id);
                            _logger.LogInformation($"Successfully auto-deleted blog: {entity.Name}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error auto-deleting blog: {entity.Name}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error initiating auto-delete for blog: {entity.Name}");
                }
            }
        }
    }
}
