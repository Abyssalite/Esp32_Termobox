using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Esp32_Control.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected readonly Store _store;
    protected readonly INavigatorService _navigator;

    protected ViewModelBase(
        Store store,
        INavigatorService navigator
    ){
        _store = store;
        _navigator = navigator;
    }
}
