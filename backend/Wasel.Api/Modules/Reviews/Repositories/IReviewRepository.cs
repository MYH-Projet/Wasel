using Wasel.Api.Modules.Reviews.Entities;

namespace Wasel.Api.Modules.Reviews.Repositories;

public interface IReviewRepository
{
    Task<bool> ExistsByDeliveryIdAsync(Guid deliveryId);
    Task<Review> AddAsync(Review review);
    Task<IEnumerable<Review>> GetByDriverIdPagedAsync(Guid driverId, int page, int pageSize);
    Task<int> CountByDriverIdAsync(Guid driverId);
    Task<double> AverageRatingByDriverIdAsync(Guid driverId);
    Task SaveChangesAsync();
}
