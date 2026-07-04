using System;
using Avalonia_Navigation;
using Websocket.Client;

namespace Esp32_Control.ViewModels;

public partial class AddDeviceViewModel : ViewModelBase
{    
    private WebsocketClient? _client;

    private string? _address;
    public string? Address
    {
        get => _address;
        set
        {
            if (value != null)
            {
                _address = value;
                _store.Address = _address;
            }
        }
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set
        {
            if (value != null)
            {
                _name = value;
                _store.Status = _name;
            }
        }
    } 

    public AddDeviceViewModel(
        Store store,
        INavigatorService navigator
    ):base(store, navigator)
    {
        Console.WriteLine("test");
    }
}
