# Umbraco Blog Sync System

This document explains how the Umbraco blog data is synced to your SQL database.

## Overview

The system provides **two sync methods**:

1. **Automatic Sync** - Blogs are automatically synced when published/unpublished in Umbraco
2. **Manual Sync** - Admin can manually sync all blogs or individual blogs via API

## How It Works

### Data Flow

```
Umbraco CMS (Content Cache)
         ↓
    Sync Service
         ↓
   SQL Database (Blogs table)
```

### Umbraco Blog Structure

```csharp
blogsItem (Content Type Alias)
├── title: Blog title
├── content: HTML content
├── excerpt: Short description
├── featuredImage: Image URL
├── publishDate: Publication date
├── tags: Comma-separated tags
├── author: Author name
└── category: Category name
```

### Database Blog Structure

```csharp
Blogs table
├── Id (GUID)
├── Title
├── Slug (from Umbraco URL segment)
├── Content
├── Excerpt
├── FeaturedImageUrl
├── AuthorId (linked to Users table)
├── CategoryId (linked to BlogCategories table)
├── IsPublished
├── PublishedAt
├── Tags (comma-separated)
├── MetaDescription (stores Umbraco NodeId)
└── ViewCount
```

## 1. Automatic Sync (Recommended)

### How It Works

When you **Save & Publish** a blog in Umbraco:
- `ContentPublishedNotificationHandler` triggers automatically
- Blog data is extracted from Umbraco
- Blog is created/updated in SQL database
- Category is auto-created if doesn't exist
- Author is matched from Users table

When you **Unpublish** a blog in Umbraco:
- `ContentUnpublishedNotificationHandler` triggers automatically
- Blog is deleted from SQL database

### Setup

The notification handlers are already registered in `NotificationComposer.cs`:

```csharp
builder.AddNotificationHandler<ContentPublishedNotification, ContentPublishedNotificationHandler>();
builder.AddNotificationHandler<ContentUnpublishedNotification, ContentUnpublishedNotificationHandler>();
```

### No Action Required

Just publish/unpublish blogs in Umbraco - sync happens automatically! ✅

## 2. Manual Sync via API

### API Endpoints

#### Sync All Blogs

**Endpoint:** `POST /api/umbracosync/sync-all-blogs`

**Auth:** Admin role required

**Response:**
```json
{
  "success": true,
  "data": {
    "syncedCount": 15,
    "message": "Successfully synced 15 blogs from Umbraco"
  }
}
```

**When to use:**
- Initial setup (sync all existing blogs)
- After database reset
- Periodic full sync to ensure consistency

#### Sync Single Blog

**Endpoint:** `POST /api/umbracosync/sync-blog/{nodeId}`

**Auth:** Admin role required

**Example:** `POST /api/umbracosync/sync-blog/1234`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "abc123...",
    "title": "Coffee Brewing Guide",
    "slug": "coffee-brewing-guide",
    "isPublished": true
  }
}
```

**When to use:**
- Sync a specific blog by its Umbraco node ID
- Re-sync after manual database changes

#### Delete Blog

**Endpoint:** `DELETE /api/umbracosync/delete-blog/{nodeId}`

**Auth:** Admin role required

**Response:**
```json
{
  "success": true,
  "data": true,
  "message": "Blog deleted successfully"
}
```

## Implementation Details

### UmbracoSyncService

The core service that handles all sync operations:

```csharp
public class UmbracoSyncService : IUmbracoSyncService
{
    // Syncs all published blogs from Umbraco
    Task<int> SyncAllBlogsAsync();

    // Syncs single blog by Umbraco node ID
    Task<BlogDto?> SyncBlogByNodeIdAsync(int nodeId);

    // Deletes blog from database
    Task<bool> DeleteBlogByNodeIdAsync(int nodeId);
}
```

### Field Mapping

| Umbraco Property | Database Field | Notes |
|-----------------|----------------|-------|
| title | Title | |
| UrlSegment | Slug | Umbraco auto-generates from title |
| content | Content | HTML content |
| excerpt | Excerpt | Short description |
| featuredImage | FeaturedImageUrl | Image URL |
| publishDate | PublishedAt | Publication date |
| tags | Tags | Comma-separated |
| author | AuthorId | Matched from Users table |
| category | CategoryId | Auto-created if doesn't exist |
| Id (node ID) | MetaDescription | Stored as "NodeId:{id}" for tracking |

### Author Matching

The system looks for authors in this order:
1. User with matching FullName
2. First user with Admin role
3. **Error if no admin found** - you must create an admin user first!

### Category Auto-Creation

If a blog references a category that doesn't exist:
- New `BlogCategory` is automatically created
- Slug is auto-generated from name
- Can be edited later via Blog Category APIs

## Initial Setup

### Step 1: Ensure Admin User Exists

```sql
-- Check if admin user exists
SELECT * FROM Users WHERE Role = 'Admin';
```

If no admin exists, create one via the User registration API first!

### Step 2: Initial Sync

Call the sync endpoint to import all existing blogs:

```bash
curl -X POST "http://localhost:5142/api/umbracosync/sync-all-blogs" \
  -H "Authorization: Bearer {your-admin-token}"
```

### Step 3: Verify Sync

Check the database:

```sql
SELECT COUNT(*) FROM Blogs;
SELECT * FROM BlogCategories;
```

## Sync Behavior

### Create vs Update

- **First sync:** Creates new blog in database
- **Subsequent syncs:** Updates existing blog (matched by slug)
- **Slug changes:** Creates new blog (old one remains)

### What Gets Synced

✅ **Synced:**
- Title, Content, Excerpt
- Featured Image URL
- Publication date
- Tags, Category
- Author

❌ **Not Synced:**
- Comments (managed separately in database)
- View count (tracked in database only)
- Blog likes/reactions (database only)

## Troubleshooting

### Issue: "Author not found" error

**Solution:** Create an admin user first:
```bash
POST /api/auth/register
{
  "email": "admin@gokcafe.com",
  "password": "YourPassword123!",
  "fullName": "Admin User",
  "role": "Admin"
}
```

### Issue: Sync not triggering automatically

**Check:**
1. NotificationComposer is registered
2. UmbracoSyncService is registered in DI
3. Check Umbraco logs for errors

### Issue: Duplicate blogs created

**Cause:** Slug changed in Umbraco

**Solution:** Delete old blog manually or keep slug consistent

### Issue: NodeId not found for deletion

**Cause:** Blog was created directly in database (not from Umbraco)

**Solution:** Delete via blog ID instead:
```bash
DELETE /api/blogs/{blogId}
```

## Best Practices

1. **Always use Umbraco CMS** to manage blogs - let automatic sync handle database
2. **Don't edit blogs directly** in SQL database - changes will be overwritten
3. **Initial setup:** Run manual sync once to import existing blogs
4. **Periodic checks:** Occasionally run full sync to ensure consistency
5. **Before deleting:** Unpublish in Umbraco first (automatic cleanup)

## Testing

### Test Automatic Sync

1. Create a new blog in Umbraco
2. Add title, content, excerpt
3. Click "Save & Publish"
4. Check database - blog should appear immediately
5. Unpublish the blog
6. Check database - blog should be deleted

### Test Manual Sync

```bash
# Get Umbraco node ID from content tree
# Sync that specific blog
curl -X POST "http://localhost:5142/api/umbracosync/sync-blog/1234" \
  -H "Authorization: Bearer {admin-token}"

# Verify in database
SELECT * FROM Blogs WHERE MetaDescription LIKE '%NodeId:1234%';
```

## Architecture

```
┌─────────────────────────────────────────────┐
│           Umbraco CMS Backend               │
│                                             │
│  ┌──────────────────────────────────────┐  │
│  │     Content Published Event          │  │
│  └─────────────┬────────────────────────┘  │
│                ↓                            │
│  ┌──────────────────────────────────────┐  │
│  │  ContentPublishedNotification        │  │
│  │  Handler (Auto Trigger)              │  │
│  └─────────────┬────────────────────────┘  │
│                │                            │
└────────────────┼────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────┐
│         UmbracoSyncService                  │
│                                             │
│  • Extract blog data from Umbraco           │
│  • Map to Blog entity                       │
│  • Create/Update in database                │
│  • Auto-create categories                   │
│  • Match authors                            │
└─────────────────┬───────────────────────────┘
                  │
                  ↓
┌─────────────────────────────────────────────┐
│         SQL Database (Azure)                │
│                                             │
│  • Blogs                                    │
│  • BlogCategories                           │
│  • BlogComments                             │
│  • Users                                    │
└─────────────────────────────────────────────┘
```

## Summary

✅ **Automatic sync** - Recommended for day-to-day use
✅ **Manual sync** - For initial setup and maintenance
✅ **Bidirectional tracking** - NodeId stored in MetaDescription
✅ **Auto-category creation** - No manual category management needed
✅ **Safe deletion** - Unpublish in Umbraco = auto-delete in database

**Remember:** Umbraco is the source of truth. Always manage blogs there! 🎉
