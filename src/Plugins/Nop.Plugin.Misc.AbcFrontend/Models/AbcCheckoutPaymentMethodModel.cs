using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Misc.AbcFrontend.Models
{
    public record AbcCheckoutPaymentMethodModel : CheckoutPaymentMethodModel
    {
        public string Description { get; set; }

        public static AbcCheckoutPaymentMethodModel FromBase(CheckoutPaymentMethodModel model)
        {
            return new AbcCheckoutPaymentMethodModel()
            {
                DisplayRewardPoints = model.DisplayRewardPoints,
                RewardPointsBalance = model.RewardPointsBalance,
                RewardPointsToUse = model.RewardPointsToUse,
                RewardPointsToUseAmount = model.RewardPointsToUseAmount,
                RewardPointsEnoughToPayForOrder = model.RewardPointsEnoughToPayForOrder,
                UseRewardPoints = model.UseRewardPoints
            };
        }
    }
}
