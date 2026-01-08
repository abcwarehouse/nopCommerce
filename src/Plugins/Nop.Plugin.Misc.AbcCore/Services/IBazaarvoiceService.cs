using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AbcCore.Services
{
    public interface IBazaarvoiceService
    {
        Task<BazaarvoiceRating> GetProductRatingAsync(int productId);
    }

    public class BazaarvoiceRating
    {
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool HasRatings => ReviewCount > 0;
    }
}
