using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Reviews.Entities;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Reviews.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly WaselDbContext _context;

    public ReviewRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByDeliveryIdAsync(Guid deliveryId)
    {
        return await _context.Reviews.AnyAsync(r => r.DeliveryId == deliveryId);
    }

    public async Task<Review> AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        return review;
    }

    public async Task<IEnumerable<Review>> GetByDriverIdPagedAsync(Guid driverId, int page, int pageSize)
    {
        return await _context.Reviews
            .Include(r => r.ReviewerUser)
            .Where(r => r.ReviewedDriverId == driverId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByDriverIdAsync(Guid driverId)
    {
        return await _context.Reviews.CountAsync(r => r.ReviewedDriverId == driverId);
    }

    public async Task<double> AverageRatingByDriverIdAsync(Guid driverId)
    {
        var hasReviews = await _context.Reviews.AnyAsync(r => r.ReviewedDriverId == driverId);
        if (!hasReviews)
            return 0;

        return await _context.Reviews
            .Where(r => r.ReviewedDriverId == driverId)
            .AverageAsync(r => r.Rating);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
