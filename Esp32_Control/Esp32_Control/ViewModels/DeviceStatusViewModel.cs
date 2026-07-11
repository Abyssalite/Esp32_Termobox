using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using Esp32_Control.Events;

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
        if (_store.SelectedDevice?.deviceStatus == null) return;

        DeviceStatus = _store.SelectedDevice.deviceStatus; 

        _subscriptions.Add(_events.Subscribe<DeviceStatusChangedEvent>( evt =>
        {
            DeviceStatus = _store.SelectedDevice.deviceStatus; 
        }));
    }
}
