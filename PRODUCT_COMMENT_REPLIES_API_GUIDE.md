# Product Comment Replies API Guide

## Overview

This guide documents the enhanced reply functionality for the Product Comments API in the GOK Cafe system. The reply system supports nested comments with depth limiting, pagination, and approval workflows.

## Key Features

- ✅ **Nested Replies**: Support for multi-level comment threads
- ✅ **Depth Limiting**: Maximum 3 levels of nesting to prevent excessive threading
- ✅ **Reply Count**: Each comment shows how many replies it has
- ✅ **Depth Tracking**: Track the nesting level of each comment/reply
- ✅ **Pagination**: Efficient loading of replies with pagination support
- ✅ **No Rating on Replies**: Only parent comments have ratings
- ✅ **Approval Workflow**: Replies require admin approval before being visible
- ✅ **Caching**: Intelligent caching for improved performance

## API Endpoints

### 1. Create a Reply

**Endpoint**: `POST /api/v1/products/{productId}/comments/replies`

**Authentication**: Required

**Request Body**:
```json
{
  "parentCommentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "comment": "This is a reply to the parent comment"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Reply created successfully. It will be visible after approval.",
  "data": {
    "id": "7b8c9d10-1234-5678-90ab-cdef12345678",
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "user-guid-here",
    "userName": "John Doe",
    "comment": "This is a reply to the parent comment",
    "rating": 0,
    "isApproved": false,
    "parentCommentId": "parent-comment-guid",
    "createdAt": "2025-12-19T10:30:00Z",
    "updatedAt": null,
    "replyCount": 0,
    "depth": 1,
    "replies": []
  }
}
```

**Notes**:
- Replies do not have ratings (always 0)
- Depth is automatically calculated based on parent comment
- Maximum depth is 3 levels
- Replies need approval before being visible to users

**Error Responses**:
- `400 Bad Request`: Parent comment not found, max depth exceeded
- `401 Unauthorized`: User not authenticated

### 2. Get Replies for a Comment

**Endpoint**: `GET /api/v1/products/{productId}/comments/{commentId}/replies`

**Authentication**: Not required (for approved replies)

**Query Parameters**:
- `pageNumber` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 10): Number of items per page
- `isApproved` (optional, default: true): Filter by approval status

**Example Request**:
```
GET /api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa6/comments/parent-comment-id/replies?pageNumber=1&pageSize=10&isApproved=true
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": null,
  "data": {
    "items": [
      {
        "id": "reply1-guid",
        "productId": "product-guid",
        "userId": "user-guid",
        "userName": "Jane Smith",
        "comment": "Great point! I agree with this.",
        "rating": 0,
        "isApproved": true,
        "parentCommentId": "parent-comment-guid",
        "createdAt": "2025-12-19T10:30:00Z",
        "updatedAt": null,
        "replyCount": 2,
        "depth": 1,
        "replies": [
          {
            "id": "nested-reply-guid",
            "productId": "product-guid",
            "userId": "another-user-guid",
            "userName": "Bob Johnson",
            "comment": "Thanks for the clarification!",
            "rating": 0,
            "isApproved": true,
            "parentCommentId": "reply1-guid",
            "createdAt": "2025-12-19T11:00:00Z",
            "updatedAt": null,
            "replyCount": 0,
            "depth": 2,
            "replies": []
          }
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalItems": 1
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid parameters
- `404 Not Found`: Parent comment not found

## Enhanced Comment DTOs

### ProductCommentDto

```typescript
{
  id: string;              // Unique identifier
  productId: string;       // Product identifier
  userId: string;          // User who created the comment
  userName: string;        // User's display name
  comment: string;         // Comment text
  rating: number;          // 1-5 stars (0 for replies)
  isApproved: boolean;     // Approval status
  parentCommentId?: string; // Parent comment ID (null for top-level)
  createdAt: string;       // ISO 8601 timestamp
  updatedAt?: string;      // ISO 8601 timestamp
  replyCount: number;      // Number of direct replies (NEW)
  depth: number;           // Nesting level (0 for top-level) (NEW)
  replies: ProductCommentDto[]; // Nested replies
}
```

### CreateReplyDto

```typescript
{
  parentCommentId: string; // REQUIRED - Parent comment ID
  comment: string;         // REQUIRED - Reply text
  // Note: No rating field - replies don't have ratings
}
```

## Reply System Rules

### 1. Depth Limiting

- **Maximum Depth**: 3 levels
- **Levels**:
  - Level 0: Top-level comments (product reviews)
  - Level 1: Replies to product reviews
  - Level 2: Replies to replies
  - Level 3: Maximum nesting (replies to level 2 replies)
- **Enforcement**: API returns error when attempting to exceed max depth

### 2. Rating Rules

- **Top-level comments**: Must have a rating (1-5 stars)
- **Replies**: Rating is always 0 (not applicable)
- **Validation**: CreateReplyDto doesn't include rating field

### 3. Approval Workflow

- **Default Status**: All replies are `isApproved = false` by default
- **Visibility**: Only approved replies are visible to regular users
- **Admin Access**: Admins can view unapproved replies using `isApproved=false` query parameter
- **Approval Endpoint**: Same as comments - `PATCH /api/v1/products/{productId}/comments/{id}/approve`

### 4. Caching Strategy

- **Reply List Cache**: Cached by parent comment ID, page, and approval status
- **TTL**: 15 minutes
- **Invalidation**: Automatically cleared when:
  - New reply is created
  - Reply is edited or deleted
  - Reply approval status changes
  - Parent comment is modified

## Usage Examples

### Example 1: Simple Reply Thread

```
Product Review (Depth 0, Rating 5)
└── Reply 1 (Depth 1, Rating 0)
    └── Reply 1.1 (Depth 2, Rating 0)
        └── Reply 1.1.1 (Depth 3, Rating 0) [MAX DEPTH]
```

### Example 2: Creating a Reply

```javascript
// POST /api/v1/products/{productId}/comments/replies
const createReply = async (parentCommentId, replyText, authToken) => {
  const response = await fetch(
    `/api/v1/products/${productId}/comments/replies`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`
      },
      body: JSON.stringify({
        parentCommentId: parentCommentId,
        comment: replyText
      })
    }
  );
  return response.json();
};
```

### Example 3: Loading Replies with Pagination

```javascript
// GET /api/v1/products/{productId}/comments/{commentId}/replies
const loadReplies = async (productId, commentId, page = 1) => {
  const response = await fetch(
    `/api/v1/products/${productId}/comments/${commentId}/replies?pageNumber=${page}&pageSize=5&isApproved=true`
  );
  return response.json();
};
```

### Example 4: Displaying Reply Count

```javascript
// Show "X replies" button
const CommentCard = ({ comment }) => {
  return (
    <div>
      <p>{comment.comment}</p>
      <p>Rating: {comment.rating > 0 ? `${comment.rating} stars` : 'N/A'}</p>
      {comment.replyCount > 0 && (
        <button onClick={() => loadReplies(comment.id)}>
          {comment.replyCount} {comment.replyCount === 1 ? 'reply' : 'replies'}
        </button>
      )}
    </div>
  );
};
```

## Frontend Integration Tips

### 1. Display Logic

```javascript
// Determine if user can reply based on depth
const canReply = (comment) => {
  return comment.depth < 3; // Max depth is 3
};

// Show rating only for top-level comments
const showRating = (comment) => {
  return comment.depth === 0 && comment.rating > 0;
};
```

### 2. Nested Comment UI

```jsx
const CommentThread = ({ comment, maxDepth = 3 }) => {
  return (
    <div style={{ marginLeft: `${comment.depth * 20}px` }}>
      <CommentCard comment={comment} />

      {comment.depth < maxDepth && (
        <ReplyButton onReply={() => createReply(comment.id)} />
      )}

      {comment.replyCount > 0 && (
        <div>
          {comment.replies.map(reply => (
            <CommentThread key={reply.id} comment={reply} maxDepth={maxDepth} />
          ))}
        </div>
      )}
    </div>
  );
};
```

### 3. Lazy Loading Replies

```javascript
// Only load replies when user clicks "Show replies"
const [showReplies, setShowReplies] = useState(false);
const [replies, setReplies] = useState([]);

const toggleReplies = async () => {
  if (!showReplies && replies.length === 0) {
    const response = await loadReplies(productId, commentId);
    setReplies(response.data.items);
  }
  setShowReplies(!showReplies);
};
```

## Testing Guide

### Test Case 1: Create a Reply
```bash
curl -X POST "http://localhost:5000/api/v1/products/{productId}/comments/replies" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "parentCommentId": "parent-comment-guid",
    "comment": "This is a test reply"
  }'
```

### Test Case 2: Verify Depth Limiting
```bash
# This should fail if trying to reply to a depth-3 comment
curl -X POST "http://localhost:5000/api/v1/products/{productId}/comments/replies" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "parentCommentId": "depth-3-comment-guid",
    "comment": "This should fail"
  }'

# Expected response:
# {
#   "success": false,
#   "message": "Maximum reply depth (3) exceeded. Cannot nest replies further."
# }
```

### Test Case 3: Get Replies with Pagination
```bash
curl "http://localhost:5000/api/v1/products/{productId}/comments/{commentId}/replies?pageNumber=1&pageSize=5&isApproved=true"
```

### Test Case 4: Admin View Unapproved Replies
```bash
curl "http://localhost:5000/api/v1/products/{productId}/comments/{commentId}/replies?isApproved=false" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

## Performance Considerations

### 1. Caching
- Replies are cached per parent comment
- Cache is invalidated on any modification
- 15-minute TTL reduces database load

### 2. Pagination
- Use pagination for comments with many replies
- Default page size: 10 replies
- Recommended: Load more replies on demand (lazy loading)

### 3. Depth Queries
- Depth calculation uses iterative approach (not recursive SQL)
- Cached depth values in DTO prevent repeated calculations
- Maximum 3 iterations (max depth limit)

## Admin Operations

Admins can use the existing comment approval endpoint for replies:

```bash
# Approve a reply
curl -X PATCH "http://localhost:5000/api/v1/products/{productId}/comments/{replyId}/approve" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "isApproved": true
  }'
```

## Migration Notes

If you have existing data with `ParentCommentId` set:
- No migration required
- Existing replies will automatically work with new endpoints
- `ReplyCount` and `Depth` are calculated dynamically
- Caching will build up naturally as endpoints are used

## Summary

The reply functionality enhancement provides:
- ✅ Dedicated reply creation endpoint
- ✅ Reply-specific DTO (no rating required)
- ✅ Pagination for replies
- ✅ Depth limiting (max 3 levels)
- ✅ Reply count tracking
- ✅ Intelligent caching
- ✅ Full integration with existing approval workflow

All endpoints are backward compatible with existing comment functionality.
