namespace Nop.Plugin.Misc.AbcCore.Delivery
{
    public record CartSlideoutInfo
    {
        public string ProductInfoHtml { get; init; }
        public string DeliveryOptionsHtml { get; init; }
        public string PickupInStoreHtml { get; init; }
        public int ShoppingCartItemId { get; init; }
        public int ProductId { get; init; }
    }
}
