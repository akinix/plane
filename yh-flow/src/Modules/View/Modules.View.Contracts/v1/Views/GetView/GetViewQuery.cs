using Mediator;
using YH.Modules.View.Contracts.DTOs;

namespace YH.Modules.View.Contracts.v1.Views.GetView;

public class GetViewQuery : IQuery<ViewDetailDto>
{
    public Guid ViewId { get; set; }
}