# Product Comments API - Quick Reference Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Authentication](#authentication)
3. [API Endpoints](#api-endpoints)
4. [Request/Response Examples](#requestresponse-examples)
5. [Error Handling](#error-handling)

---

## Overview

The Product Comments API allows users to leave ratings and reviews on products. Comments require admin approval before being publicly visible and support nested replies.

**Base URL:** `/api/v1`

**Features:**
- ⭐ 1-5 star ratings
- 💬 Text comments
- 🔒 Authentication required for write operations
- ✅ Admin approval workflow
- 🔁 Nested replies
- 📊 Rating statistics and summaries

---

## Authentication

Most endpoints require JWT Bearer token authentication.

**How to authenticate:**
1. Register or login to get a token:
   - `POST /api/v1/auth/register`
   - `POST /api/v1/auth/login`

2. Include token in headers:
   ```
   Authorization: Bearer YOUR_JWT_TOKEN_HERE
   ```

---

## API Endpoints

### 1️⃣ Create Comment
Create a new comment or reply on a product.

```http
POST /api/v1/products/{productId}/comments
```

**Authentication:** ✅ Required
**Request Body:**
```json
{
  "comment": "This coffee is amazing!",
  "rating": 5,
  "parentCommentId": null  // Optional: For replies
}
```

**Response:**
```json
{
  "success": true,
  "message": "Comment created successfully. It will be visible after approval.",
  "data": {
    "id": "guid",
    "productId": "guid",
    "userId": "guid",
    "userName": "John Doe",
    "comment": "This coffee is amazing!",
    "rating": 5,
    "isApproved": false,
    "parentCommentId": null,
    "createdAt": "2025-12-18T08:00:00Z",
    "updatedAt": null,
    "replies": []
  }
}
```

---

### 2️⃣ Get Product Comments
Retrieve all comments for a specific product.

```http
GET /api/v1/products/{productId}/comments?pageNumber=1&pageSize=10&isApproved=true
```

**Authentication:** ❌ Not Required
**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `isApproved` (default: true) - Filter by approval status

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userName": "John Doe",
        "comment": "Great product!",
        "rating": 5,
        "isApproved": true,
        "createdAt": "2025-12-18T08:00:00Z",
        "replies": [...]
      }
    ],
    "totalItems": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

---

### 3️⃣ Get Comment Summary
Get rating statistics for a product.

```http
GET /api/v1/products/{productId}/comments/summary
```

**Authentication:** ❌ Not Required
**Response:**
```json
{
  "success": true,
  "data": {
    "totalComments": 25,
    "averageRating": 4.6,
    "ratingDistribution": {
      "1": 0,
      "2": 1,
      "3": 2,
      "4": 7,
      "5": 15
    }
  }
}
```

---

### 4️⃣ Get Specific Comment
Retrieve a single comment by ID (includes replies).

```http
GET /api/v1/products/{productId}/comments/{commentId}
```

**Authentication:** ❌ Not Required
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "comment": "Great coffee!",
    "rating": 5,
    "replies": [
      {
        "id": "guid",
        "comment": "Thank you!",
        "rating": 5,
        "parentCommentId": "parent-guid"
      }
    ]
  }
}
```

---

### 5️⃣ Update Comment
Update your own comment.

```http
PUT /api/v1/products/{productId}/comments/{commentId}
```

**Authentication:** ✅ Required (must be comment owner)
**Request Body:**
```json
{
  "comment": "Updated comment text",
  "rating": 4
}
```

**Response:**
```json
{
  "success": true,
  "message": "Comment updated successfully. It will be visible after approval.",
  "data": { ... }
}
```

**Note:** Approval status is reset to `false` after editing.

---

### 6️⃣ Delete Comment
Soft delete your own comment.

```http
DELETE /api/v1/products/{productId}/comments/{commentId}
```

**Authentication:** ✅ Required (must be comment owner)
**Response:**
```json
{
  "success": true,
  "message": "Comment deleted successfully",
  "data": true
}
```

---

### 7️⃣ Get My Comments
Retrieve all comments created by the authenticated user.

```http
GET /api/v1/my-comments?pageNumber=1&pageSize=10
```

**Authentication:** ✅ Required
**Response:**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalItems": 10,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

---

### 8️⃣ Approve/Disapprove Comment (Admin Only)
Moderate comments by approving or disapproving them.

```http
PATCH /api/v1/products/{productId}/comments/{commentId}/approve
```

**Authentication:** ✅ Required (Admin role only)
**Request Body:**
```json
{
  "isApproved": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Comment approved successfully",
  "data": true
}
```

---

## Request/Response Examples

### Example 1: Customer leaves a 5-star review

**Request:**
```bash
curl -X POST "http://localhost:5142/api/v1/products/{productId}/comments" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "comment": "Absolutely love this coffee! The aroma is incredible and the taste is smooth and rich.",
    "rating": 5
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Comment created successfully. It will be visible after approval.",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "userName": "Jane Smith",
    "comment": "Absolutely love this coffee!...",
    "rating": 5,
    "isApproved": false
  }
}
```

---

### Example 2: Get product rating overview

**Request:**
```bash
curl "http://localhost:5142/api/v1/products/{productId}/comments/summary"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalComments": 42,
    "averageRating": 4.8,
    "ratingDistribution": {
      "1": 0,
      "2": 0,
      "3": 2,
      "4": 8,
      "5": 32
    }
  }
}
```

This shows that out of 42 reviews:
- 32 are 5-star ⭐⭐⭐⭐⭐
- 8 are 4-star ⭐⭐⭐⭐
- 2 are 3-star ⭐⭐⭐
- Average: 4.8/5

---

### Example 3: Create a reply to an existing comment

**Request:**
```bash
curl -X POST "http://localhost:5142/api/v1/products/{productId}/comments" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "comment": "Thank you for your feedback! We are glad you enjoyed it.",
    "rating": 5,
    "parentCommentId": "123e4567-e89b-12d3-a456-426614174000"
  }'
```

---

## Error Handling

All errors follow a consistent format:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Additional error details"]
}
```

### Common Error Scenarios

#### 1. Invalid Rating
```json
{
  "success": false,
  "message": "Rating must be between 1 and 5"
}
```

#### 2. Unauthorized Access
HTTP 401 - Missing or invalid authentication token

#### 3. Comment Not Found
```json
{
  "success": false,
  "message": "Comment not found"
}
```

#### 4. Not Comment Owner
```json
{
  "success": false,
  "message": "Unauthorized to update this comment"
}
```

#### 5. Product Not Found
```json
{
  "success": false,
  "message": "Product not found"
}
```

#### 6. Invalid Parent Comment
```json
{
  "success": false,
  "message": "Invalid parent comment"
}
```

---

## Validation Rules

| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `comment` | ✅ Yes | string | Max 1000 characters |
| `rating` | ✅ Yes | integer | Between 1 and 5 |
| `parentCommentId` | ❌ No | guid | Must be valid existing comment ID |
| `productId` | ✅ Yes | guid | Must be valid existing product ID |

---

## Best Practices

### For Frontend Developers

1. **Display Approval Status**
   - Show users that their comment is "pending approval"
   - Don't display unapproved comments to other users

2. **Show Rating Distribution**
   - Use the summary endpoint to display star ratings visually
   - Show percentage breakdown of ratings

3. **Implement Pagination**
   - Load comments in batches (10-20 per page)
   - Implement "Load More" or pagination controls

4. **Handle Nested Replies**
   - Display replies indented or threaded
   - Limit reply depth if needed (e.g., max 3 levels)

5. **Real-time Updates**
   - Refresh comment count after new comment
   - Show "Your comment is pending approval" message

### For Admins

1. **Moderate Comments**
   - Review unapproved comments regularly
   - Use the approve endpoint to make comments visible

2. **Monitor Quality**
   - Check for spam or inappropriate content
   - Use the comment filtering to find unapproved comments

3. **Engagement**
   - Reply to customer comments
   - Address concerns raised in reviews

---

## Testing the API

You can test the API using:
- **Swagger UI:** `http://localhost:5142/swagger`
- **Postman:** Import the endpoints
- **curl:** Command-line testing (see examples above)

---

## Support

For issues or questions:
- Check the test report: `PRODUCT_COMMENTS_API_TEST_REPORT.md`
- Review API endpoints in Swagger UI
- Ensure proper authentication headers are included
