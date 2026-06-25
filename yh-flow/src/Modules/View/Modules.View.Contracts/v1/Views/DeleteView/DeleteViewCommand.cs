using Mediator;

namespace YH.Modules.View.Contracts.v1.Views.DeleteView;

public class DeleteViewCommand : ICommand<bool>
{
    public Guid ViewId { get; set; }
}