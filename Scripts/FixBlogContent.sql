-- =============================================
-- Fix Blog Content - Replace BlockGridModel placeholders
-- Description: Updates blog content that shows "Umbraco.Cms.Core.Models.Blocks.BlockGridModel"
--              with placeholder HTML content until proper sync can occur
-- =============================================

-- Update blogs with BlockGridModel placeholder
UPDATE [dbo].[Blogs]
SET [Content] = '<p>This blog content is currently being migrated from Umbraco. Please check back soon for the full content.</p>'
WHERE [Content] = 'Umbraco.Cms.Core.Models.Blocks.BlockGridModel';

-- Show results
SELECT
    [Id],
    [Title],
    [Slug],
    LEFT([Content], 100) as ContentPreview,
    [UpdatedAt]
FROM [dbo].[Blogs]
WHERE [Content] LIKE '%migrated from Umbraco%';

PRINT 'Blog content has been updated. Blogs with BlockGridModel placeholder have been replaced with temporary content.';
PRINT 'To get the actual content, you need to:';
PRINT '1. Ensure Umbraco CMS is running with the blog content';
PRINT '2. Call the sync endpoint: POST /api/v1/blog-sync/sync-all';
PRINT '3. The fixed UmbracoSyncService will properly extract HTML from BlockGridModel';

GO
