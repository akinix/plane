using YH.Framework.Shared.Quota;

namespace YH.Modules.Billing.Contracts.Dtos;

public sealed record BillingPlanDto(
    Guid Id,
    string Key,
    string Name,
    string Currency,
    decimal MonthlyBasePrice,
    IReadOnlyDictionary<QuotaResource, decimal> OverageRates,
    bool IsActive,
    PlanInterval Interval,
    decimal? AnnualPrice);
