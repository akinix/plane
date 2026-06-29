using Mediator;

namespace YH.Modules.View.Contracts.v1.Views.ArchiveView;

public class ArchiveViewCommand : ICommand<bool>
{
    public Guid ViewId { get; set; }
}

public class UnarchiveViewCommand : ICommand<bool>
{
    public Guid ViewId { get; set; }
}