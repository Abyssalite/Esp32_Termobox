using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using Esp32_Control.Events;

namespace Esp32_Control.ViewModels;

public partial class DeviceSettingViewModel : ViewModelBase
{    
    [ObservableProperty]
    private DeviceStatus? deviceStatus = new();
    private float? _setTemp = 1;
    public float? SetTemp
    {
        get => _setTemp;
        set
        {
            if (value == _setTemp) return;
            _setTemp = value;
            OnPropertyChanged(nameof(SetTemp));
            _events.Publish(new SettingChangedEvent("SetTemp", _setTemp ?? 1));
        }
    }
    private int? _fan1Speed = 1;
    public int? Fan1Speed
    {
        get => _fan1Speed;
        set
        {
            if (value == _fan1Speed) return;
            _fan1Speed = value ?? 10;
            OnPropertyChanged(nameof(Fan1Speed));
            _events.Publish(new SettingChangedEvent("Fan1Speed", _fan1Speed ?? 20));
        }
    }
    private int? _fan2Speed = 1;
    public int? Fan2Speed
    {
        get => _fan2Speed;
        set
        {
            if (value == _fan2Speed) return;
            _fan2Speed = value ?? 10;
            OnPropertyChanged(nameof(Fan2Speed));
            _events.Publish(new SettingChangedEvent("Fan2Speed", _fan2Speed ?? 20));
        }
    }
    private int? _tecPower = 1;
    public int? TecPower
    {
        get => _tecPower;
        set
        {
            if (value == _tecPower) return; 
            _tecPower = value ?? 10;
            OnPropertyChanged(nameof(TecPower));
            _events.Publish(new SettingChangedEvent("TecPower", _tecPower ?? 10));
        }
    }

    public DeviceSettingViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {

        if (_store.SelectedDevice?.deviceStatus == null) return;

        DeviceStatus = _store.SelectedDevice.deviceStatus; 

        _subscriptions.Add(_events.Subscribe<DeviceStatusChangedEvent>( evt =>
        {
            SetTemp = _store.SelectedDevice.deviceStatus.SetTemp;
            Fan1Speed = _store.SelectedDevice.deviceStatus.Fan1Speed;
            Fan2Speed = _store.SelectedDevice.deviceStatus.Fan2Speed;
            TecPower = _store.SelectedDevice.deviceStatus.TecPower;
        }));
    }
}
