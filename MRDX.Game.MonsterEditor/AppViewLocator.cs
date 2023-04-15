using ReactiveUI;

namespace MRDX.Game.MonsterEditor;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T viewModel, string? contract = null)
    {
        return viewModel switch
        {
            WaitForGameLoadModel context => new WaitForGameLoadView { DataContext = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
        };
    }
}