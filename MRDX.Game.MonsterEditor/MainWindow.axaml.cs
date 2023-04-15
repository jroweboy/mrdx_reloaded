using System.Reactive;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace MRDX.Game.MonsterEditor;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
        DataContext = new MainWindowViewModel();
    }
}

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public MainWindowViewModel()
    {
        // Manage the routing state. Use the Router.Navigate.Execute
        // command to navigate to different view models. 
        //
        // Note, that the Navigate.Execute method accepts an instance 
        // of a view model, this allows you to pass parameters to 
        // your view models, or to reuse existing view models.
        //
        // GoNext = ReactiveCommand.CreateFromObservable(
        //     () => Router.Navigate.Execute(new WaitForGameLoadModel(this))
        // );
    }

    // The command that navigates a user to first view model.
    // public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }
    //
    // // The command that navigates a user back.
    // public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; } = new();
}