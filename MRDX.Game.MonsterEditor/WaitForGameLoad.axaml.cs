using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace MRDX.Game.MonsterEditor;

public partial class WaitForGameLoadView : ReactiveUserControl<WaitForGameLoadModel>
{
    public WaitForGameLoadView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class WaitForGameLoadModel : ReactiveObject, IRoutableViewModel
{
    public WaitForGameLoadModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }
}