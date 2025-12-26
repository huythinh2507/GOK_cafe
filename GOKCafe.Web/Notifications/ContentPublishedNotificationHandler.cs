using GOKCafe.Web.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace GOKCafe.Web.Notifications;

/// <summary>
/// Automatically sync blog to database when published in Umbraco
/// </summary>
public class ContentPublishedNotificationHandler : INotificationHandler<ContentPublishedNotification>
{
    private readonly IUmbracoSyncService _umbracoSyncService;
    private readonly ILogger<ContentPublishedNotificationHandler> _logger;

    public ContentPublishedNotificationHandler(
        IUmbracoSyncService umbracoSyncService,
        ILogger<ContentPublishedNotificationHandler> logger)
    {
        _umbracoSyncService = umbracoSyncService;
        _logger = logger;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        foreach (var entity in notification.PublishedEntities)
        {
            // Only sync if it's a blog item
            if (entity.ContentType.Alias == "blogsItem")
            {
                try
                {
                    _logger.LogInformation($"Auto-syncing published blog: {entity.Name} (ID: {entity.Id})");

                    // Run sync asynchronously without blocking Umbraco
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _umbracoSyncService.SyncBlogByNodeIdAsync(entity.Id);
                            _logger.LogInformation($"Successfully auto-synced blog: {entity.Name}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error auto-syncing blog: {entity.Name}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error initiating auto-sync for blog: {entity.Name}");
                }
            }
        }
    }
}
