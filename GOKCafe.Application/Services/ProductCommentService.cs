using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductComment;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GOKCafe.Application.Services;

public class ProductCommentService : IProductCommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CommentCacheKeyPrefix = "comment:";
    private const string ProductCommentsCacheKeyPrefix = "product-comments:";
    private const string CommentRepliesCacheKeyPrefix = "comment-replies:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);
    private const int MaxReplyDepth = 3; // Maximum nesting level for replies

    public ProductCommentService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetProductCommentsAsync(
        Guid productId,
        ProductCommentFilterDto filter)
    {
        try
        {
            // Build cache key including all filter parameters
            var ratingsKey = filter.Ratings != null && filter.Ratings.Any()
                ? string.Join("-", filter.Ratings.OrderBy(r => r))
                : "all";
            var ratingFilter = filter.Ratings;
            var cacheKey = $"{ProductCommentsCacheKeyPrefix}{productId}:page:{filter.PageNumber}:size:{filter.PageSize}:approved:{filter.IsApproved}:ratings:{ratingsKey}:hasReplies:{filter.HasReplies}:hasImages:{filter.HasImages}:search:{filter.Search}";

            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<ProductCommentDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            Expression<Func<ProductComment, bool>> predicate = c => c.ProductId == productId && c.ParentCommentId == null;

            if (ratingFilter != null && ratingFilter.Count != 0)
                predicate = c => c.ProductId == productId && c.ParentCommentId == null && ratingFilter.Contains(c.Rating);

            var query = _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(predicate);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(c => c.Comment.Contains(filter.Search));

            var totalItems = await query.CountAsync();
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var commentDtos = comments.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<ProductCommentDto>>.SuccessResult(
                new PaginatedResponse<ProductCommentDto>
                {
                    Items = commentDtos,
                    TotalItems = totalItems,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductCommentDto>>.FailureResult(
                $"Error retrieving comments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductCommentDto>> GetCommentByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{CommentCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ProductCommentDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var comment = await _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return ApiResponse<ProductCommentDto>.FailureResult("Comment not found");

            var commentDto = MapToDto(comment);
            var response = ApiResponse<ProductCommentDto>.SuccessResult(commentDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCommentDto>.FailureResult(
                $"Error retrieving comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductCommentDto>> CreateCommentAsync(Guid userId, CreateProductCommentDto dto)
    {
        try
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                return ApiResponse<ProductCommentDto>.FailureResult("Product not found");

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<ProductCommentDto>.FailureResult("User not found");

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return ApiResponse<ProductCommentDto>.FailureResult("Rating must be between 1 and 5");

            // Validate parent comment if specified
            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _unitOfWork.ProductComments.GetByIdAsync(dto.ParentCommentId.Value);
                if (parentComment == null || parentComment.ProductId != dto.ProductId)
                    return ApiResponse<ProductCommentDto>.FailureResult("Invalid parent comment");
            }

            var comment = new ProductComment
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                UserId = userId,
                Comment = dto.Comment,
                Rating = dto.Rating,
                ParentCommentId = dto.ParentCommentId,
                IsApproved = false, // Comments need approval by default
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductComments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await InvalidateProductCommentsCache(dto.ProductId);

            // Load related data for response
            comment = await _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            var commentDto = MapToDto(comment!);
            return ApiResponse<ProductCommentDto>.SuccessResult(
                commentDto,
                "Comment created successfully. It will be visible after approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCommentDto>.FailureResult(
                $"Error creating comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductCommentDto>> UpdateCommentAsync(
        Guid id,
        Guid userId,
        UpdateProductCommentDto dto)
    {
        try
        {
            var comment = await _unitOfWork.ProductComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<ProductCommentDto>.FailureResult("Comment not found");

            // Only the comment owner can update
            if (comment.UserId != userId)
                return ApiResponse<ProductCommentDto>.FailureResult("Unauthorized to update this comment");

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return ApiResponse<ProductCommentDto>.FailureResult("Rating must be between 1 and 5");

            comment.Comment = dto.Comment;
            comment.Rating = dto.Rating;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsApproved = false; // Reset approval status after edit

            _unitOfWork.ProductComments.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCommentCache(id);
            await InvalidateProductCommentsCache(comment.ProductId);

            // Load related data for response
            comment = await _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            var commentDto = MapToDto(comment!);
            return ApiResponse<ProductCommentDto>.SuccessResult(
                commentDto,
                "Comment updated successfully. It will be visible after approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCommentDto>.FailureResult(
                $"Error updating comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(Guid id, Guid userId)
    {
        try
        {
            var comment = await _unitOfWork.ProductComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<bool>.FailureResult("Comment not found");

            // Only the comment owner can delete
            if (comment.UserId != userId)
                return ApiResponse<bool>.FailureResult("Unauthorized to delete this comment");

            _unitOfWork.ProductComments.SoftDelete(comment);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCommentCache(id);
            await InvalidateProductCommentsCache(comment.ProductId);

            return ApiResponse<bool>.SuccessResult(true, "Comment deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult($"Error deleting comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ApproveCommentAsync(Guid id, ApproveProductCommentDto dto)
    {
        try
        {
            var comment = await _unitOfWork.ProductComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<bool>.FailureResult("Comment not found");

            comment.IsApproved = dto.IsApproved;
            comment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductComments.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCommentCache(id);
            await InvalidateProductCommentsCache(comment.ProductId);

            return ApiResponse<bool>.SuccessResult(
                true,
                dto.IsApproved ? "Comment approved successfully" : "Comment disapproved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult($"Error approving comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductCommentSummaryDto>> GetProductCommentSummaryAsync(Guid productId)
    {
        try
        {
            var cacheKey = $"{ProductCommentsCacheKeyPrefix}{productId}:summary";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ProductCommentSummaryDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var comments = await _unitOfWork.ProductComments.GetQueryable()
                .Where(c => c.ProductId == productId && c.IsApproved && c.ParentCommentId == null)
                .ToListAsync();

            var totalComments = comments.Count;
            var averageRating = totalComments > 0 ? comments.Average(c => c.Rating) : 0;

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = comments.Count(c => c.Rating == i);
            }

            var summary = new ProductCommentSummaryDto
            {
                TotalComments = totalComments,
                AverageRating = Math.Round(averageRating, 1),
                RatingDistribution = ratingDistribution
            };

            var response = ApiResponse<ProductCommentSummaryDto>.SuccessResult(summary);
            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCommentSummaryDto>.FailureResult(
                $"Error retrieving comment summary: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetUserCommentsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var query = _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.Product)
                .Include(c => c.User)
                .Where(c => c.UserId == userId);

            var totalItems = await query.CountAsync();
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var commentDtos = comments.Select(MapToDto).ToList();

            return ApiResponse<PaginatedResponse<ProductCommentDto>>.SuccessResult(
                new PaginatedResponse<ProductCommentDto>
                {
                    Items = commentDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductCommentDto>>.FailureResult(
                $"Error retrieving user comments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductCommentDto>> CreateReplyAsync(Guid userId, CreateReplyDto dto)
    {
        try
        {
            // Validate parent comment exists
            var parentComment = await _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId);

            if (parentComment == null)
                return ApiResponse<ProductCommentDto>.FailureResult("Parent comment not found");

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<ProductCommentDto>.FailureResult("User not found");

            // Check reply depth to prevent excessive nesting
            var depth = await GetCommentDepthAsync(dto.ParentCommentId);
            if (depth >= MaxReplyDepth)
                return ApiResponse<ProductCommentDto>.FailureResult(
                    $"Maximum reply depth ({MaxReplyDepth}) exceeded. Cannot nest replies further.");

            var reply = new ProductComment
            {
                Id = Guid.NewGuid(),
                ProductId = parentComment.ProductId,
                UserId = userId,
                Comment = dto.Comment,
                Rating = 0, // Replies don't have ratings
                ParentCommentId = dto.ParentCommentId,
                IsApproved = false, // Replies need approval by default
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductComments.AddAsync(reply);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCommentCache(dto.ParentCommentId);
            await InvalidateProductCommentsCache(parentComment.ProductId);
            await InvalidateRepliesCache(dto.ParentCommentId);

            // Load related data for response
            reply = await _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == reply.Id);

            var replyDto = MapToDto(reply!, depth + 1);
            return ApiResponse<ProductCommentDto>.SuccessResult(
                replyDto,
                "Reply created successfully. It will be visible after approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCommentDto>.FailureResult(
                $"Error creating reply: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetRepliesAsync(
        Guid parentCommentId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = true)
    {
        try
        {
            var cacheKey = $"{CommentRepliesCacheKeyPrefix}{parentCommentId}:page:{pageNumber}:size:{pageSize}:approved:{isApproved}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<ProductCommentDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            // Validate parent comment exists
            var parentComment = await _unitOfWork.ProductComments.GetByIdAsync(parentCommentId);
            if (parentComment == null)
                return ApiResponse<PaginatedResponse<ProductCommentDto>>.FailureResult("Parent comment not found");

            var query = _unitOfWork.ProductComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.ParentCommentId == parentCommentId);

            if (isApproved.HasValue)
                query = query.Where(c => c.IsApproved == isApproved.Value);

            var totalItems = await query.CountAsync();
            var replies = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get the depth of the parent comment
            var parentDepth = await GetCommentDepthAsync(parentCommentId);
            var replyDtos = replies.Select(r => MapToDto(r, parentDepth + 1)).ToList();

            var response = ApiResponse<PaginatedResponse<ProductCommentDto>>.SuccessResult(
                new PaginatedResponse<ProductCommentDto>
                {
                    Items = replyDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductCommentDto>>.FailureResult(
                $"Error retrieving replies: {ex.Message}");
        }
    }

    private ProductCommentDto MapToDto(ProductComment comment, int depth = 0)
    {
        return new ProductCommentDto
        {
            Id = comment.Id,
            ProductId = comment.ProductId,
            UserId = comment.UserId,
            UserName = $"{comment.User.FirstName} {comment.User.LastName}",
            Comment = comment.Comment,
            Rating = comment.Rating,
            IsApproved = comment.IsApproved,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ReplyCount = comment.Replies?.Count ?? 0,
            Depth = depth,
            Replies = comment.Replies?.Select(r => MapToDto(r, depth + 1)).ToList() ?? new List<ProductCommentDto>()
        };
    }

    private async Task InvalidateCommentCache(Guid commentId)
    {
        var cacheKey = $"{CommentCacheKeyPrefix}{commentId}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    private async Task InvalidateProductCommentsCache(Guid productId)
    {
        // Note: In a production system, you'd want a more sophisticated cache invalidation strategy
        // For now, we'll just clear specific keys
        var approvedKey = $"{ProductCommentsCacheKeyPrefix}{productId}:page:1:size:10:approved:True";
        var allKey = $"{ProductCommentsCacheKeyPrefix}{productId}:page:1:size:10:approved:";
        var summaryKey = $"{ProductCommentsCacheKeyPrefix}{productId}:summary";

        await _cacheService.RemoveAsync(approvedKey);
        await _cacheService.RemoveAsync(allKey);
        await _cacheService.RemoveAsync(summaryKey);
    }

    private async Task InvalidateRepliesCache(Guid parentCommentId)
    {
        var approvedKey = $"{CommentRepliesCacheKeyPrefix}{parentCommentId}:page:1:size:10:approved:True";
        var allKey = $"{CommentRepliesCacheKeyPrefix}{parentCommentId}:page:1:size:10:approved:";

        await _cacheService.RemoveAsync(approvedKey);
        await _cacheService.RemoveAsync(allKey);
    }

    private async Task<int> GetCommentDepthAsync(Guid commentId)
    {
        var depth = 0;
        var currentCommentId = commentId;

        while (currentCommentId != Guid.Empty)
        {
            var comment = await _unitOfWork.ProductComments.GetByIdAsync(currentCommentId);
            if (comment == null || !comment.ParentCommentId.HasValue)
                break;

            depth++;
            currentCommentId = comment.ParentCommentId.Value;
        }

        return depth;
    }
}
