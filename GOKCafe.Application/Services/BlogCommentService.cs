using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class BlogCommentService : IBlogCommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CommentCacheKeyPrefix = "blog-comment:";
    private const string BlogCommentsCacheKeyPrefix = "blog-comments:";
    private const int MaxReplyDepth = 3;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public BlogCommentService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<BlogCommentDto>>> GetBlogCommentsAsync(
        Guid blogId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = true)
    {
        try
        {
            var cacheKey = $"{BlogCommentsCacheKeyPrefix}{blogId}:page:{pageNumber}:size:{pageSize}:approved:{isApproved}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<BlogCommentDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.BlogComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.BlogId == blogId && c.ParentCommentId == null);

            if (isApproved.HasValue)
                query = query.Where(c => c.IsApproved == isApproved.Value);

            var totalItems = await query.CountAsync();
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var commentDtos = comments.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<BlogCommentDto>>.SuccessResult(
                new PaginatedResponse<BlogCommentDto>
                {
                    Items = commentDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<BlogCommentDto>>.FailureResult(
                $"Error retrieving comments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCommentDto>> GetCommentByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{CommentCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<BlogCommentDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var comment = await _unitOfWork.BlogComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return ApiResponse<BlogCommentDto>.FailureResult("Comment not found");

            var commentDto = MapToDto(comment);
            var response = ApiResponse<BlogCommentDto>.SuccessResult(commentDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCommentDto>.FailureResult(
                $"Error retrieving comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCommentDto>> CreateCommentAsync(CreateBlogCommentDto dto, Guid userId)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(dto.BlogId);
            if (blog == null)
                return ApiResponse<BlogCommentDto>.FailureResult("Blog not found");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<BlogCommentDto>.FailureResult("User not found");

            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _unitOfWork.BlogComments.GetByIdAsync(dto.ParentCommentId.Value);
                if (parentComment == null || parentComment.BlogId != dto.BlogId)
                    return ApiResponse<BlogCommentDto>.FailureResult("Invalid parent comment");

                var depth = await GetCommentDepth(dto.ParentCommentId.Value);
                if (depth >= MaxReplyDepth)
                    return ApiResponse<BlogCommentDto>.FailureResult($"Maximum reply depth of {MaxReplyDepth} exceeded");
            }

            var comment = new BlogComment
            {
                Id = Guid.NewGuid(),
                BlogId = dto.BlogId,
                UserId = userId,
                Comment = dto.Comment,
                ParentCommentId = dto.ParentCommentId,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BlogComments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCommentsCache(dto.BlogId);

            var createdComment = await _unitOfWork.BlogComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            var commentDto = MapToDto(createdComment!);
            return ApiResponse<BlogCommentDto>.SuccessResult(commentDto, "Comment created successfully. Awaiting approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCommentDto>.FailureResult(
                $"Error creating comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCommentDto>> UpdateCommentAsync(Guid id, UpdateBlogCommentDto dto, Guid userId)
    {
        try
        {
            var comment = await _unitOfWork.BlogComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<BlogCommentDto>.FailureResult("Comment not found");

            if (comment.UserId != userId)
                return ApiResponse<BlogCommentDto>.FailureResult("You can only edit your own comments");

            comment.Comment = dto.Comment;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsApproved = false;

            _unitOfWork.BlogComments.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCommentsCache(comment.BlogId);

            var updatedComment = await _unitOfWork.BlogComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            var commentDto = MapToDto(updatedComment!);
            return ApiResponse<BlogCommentDto>.SuccessResult(commentDto, "Comment updated successfully. Awaiting re-approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCommentDto>.FailureResult(
                $"Error updating comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(Guid id, Guid userId)
    {
        try
        {
            var comment = await _unitOfWork.BlogComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<bool>.FailureResult("Comment not found");

            if (comment.UserId != userId)
                return ApiResponse<bool>.FailureResult("You can only delete your own comments");

            _unitOfWork.BlogComments.SoftDelete(comment);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCommentsCache(comment.BlogId);

            return ApiResponse<bool>.SuccessResult(true, "Comment deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error deleting comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ApproveCommentAsync(Guid id)
    {
        try
        {
            var comment = await _unitOfWork.BlogComments.GetByIdAsync(id);
            if (comment == null)
                return ApiResponse<bool>.FailureResult("Comment not found");

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.BlogComments.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCommentsCache(comment.BlogId);

            return ApiResponse<bool>.SuccessResult(true, "Comment approved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error approving comment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedResponse<BlogCommentDto>>> GetUserCommentsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var cacheKey = $"user-blog-comments:{userId}:page:{pageNumber}:size:{pageSize}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<BlogCommentDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.BlogComments.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.UserId == userId);

            var totalItems = await query.CountAsync();
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var commentDtos = comments.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<BlogCommentDto>>.SuccessResult(
                new PaginatedResponse<BlogCommentDto>
                {
                    Items = commentDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<BlogCommentDto>>.FailureResult(
                $"Error retrieving user comments: {ex.Message}");
        }
    }

    private BlogCommentDto MapToDto(BlogComment comment)
    {
        return new BlogCommentDto
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            UserName = comment.User != null ? $"{comment.User.FirstName} {comment.User.LastName}".Trim() : string.Empty,
            Comment = comment.Comment,
            IsApproved = comment.IsApproved,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Replies = comment.Replies?
                .Where(r => r.IsApproved)
                .Select(MapToDto)
                .OrderBy(r => r.CreatedAt)
                .ToList() ?? new List<BlogCommentDto>()
        };
    }

    private async Task<int> GetCommentDepth(Guid commentId)
    {
        int depth = 0;
        var currentCommentId = commentId;

        while (currentCommentId != Guid.Empty)
        {
            var comment = await _unitOfWork.BlogComments.GetByIdAsync(currentCommentId);
            if (comment?.ParentCommentId == null)
                break;

            depth++;
            currentCommentId = comment.ParentCommentId.Value;
        }

        return depth;
    }

    private async Task InvalidateBlogCommentsCache(Guid blogId)
    {
        await _cacheService.RemoveAsync($"{BlogCommentsCacheKeyPrefix}{blogId}");
    }
}
