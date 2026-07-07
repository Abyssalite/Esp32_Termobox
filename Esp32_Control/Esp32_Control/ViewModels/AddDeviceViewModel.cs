using System;
using Avalonia_Navigation;
using Websocket.Client;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Esp32_Control.ViewModels;

public partial class AddDeviceViewModel : ViewModelBase
{    
    private WebsocketClient? _client;
    public ICommand? BackCommand { get; }

    public string? Address { get; set; }
    public string? Name { get; set; }


    public AddDeviceViewModel(
        Store store,
        INavigatorService navigator
    ):base(store, navigator)
    {
        BackCommand = new AsyncRelayCommand(() => _navigator.OpenPrevious());

    }
}
