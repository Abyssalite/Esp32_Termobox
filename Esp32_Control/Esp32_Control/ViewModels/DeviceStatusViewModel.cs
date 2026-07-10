using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esp32_Control.Events;
using Websocket.Client;


namespace Esp32_Control.ViewModels;

public partial class DeviceStatusViewModel : ViewModelBase
{    

    [ObservableProperty]
    private DeviceStatus? deviceStatus = new();
    
    public DeviceStatusViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {
        _subscriptions.Add(_events.Subscribe<DeviceStatusChangedEvent>( evt =>
        {
            DeviceStatus = _store.deviceStatus;
        }));
    }
}
