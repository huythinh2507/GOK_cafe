using GOKCafe.Web.Notifications;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;

namespace GOKCafe.Web.Composers;

public class NotificationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register notification handlers for automatic blog sync
        builder.AddNotificationHandler<ContentPublishedNotification, ContentPublishedNotificationHandler>();
        builder.AddNotificationHandler<ContentUnpublishedNotification, ContentUnpublishedNotificationHandler>();
    }
}
