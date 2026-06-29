using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycleProgress;
using YH.Modules.WorkItems.Services;

namespace YH.Modules.WorkItems.Features.v1.Cycles.GetCycleProgress;

/// <summary>
/// Handles <see cref="GetCycleProgressQuery"/> — returns the completion chart for a cycle.
/// </summary>
public sealed class GetCycleProgressQueryHandler : IQueryHandler<GetCycleProgressQuery, CycleProgressDto>
{
    private readonly IBurndownCalculator _burndown;

    public GetCycleProgressQueryHandler(IBurndownCalculator burndown)
    {
        _burndown = burndown ?? throw new ArgumentNullException(nameof(burndown));
    }

    public async ValueTask<CycleProgressDto> Handle(GetCycleProgressQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var chart = await _burndown.CalculateCompletionChartAsync(query.CycleId, query.Type ?? "issues", cancellationToken)
            .ConfigureAwait(false);

        return new CycleProgressDto
        {
            CompletionChart = chart
        };
    }
}
