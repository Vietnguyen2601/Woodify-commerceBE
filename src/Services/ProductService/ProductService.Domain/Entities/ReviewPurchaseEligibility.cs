namespace ProductService.Domain.Entities;

/// <summary>
/// Rows ingested from OrderReviewEligibleEvent (order.events / order.review_eligible) — buyer may post a review for this order line.
/// </summary>
public class ReviewPurchaseEligibility
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }

    public DateTime EligibleAt { get; set; }

    public bool IsConsumed { get; set; }
    public Guid? ReviewId { get; set; }
}
