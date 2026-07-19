using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using Esp32_Control.Events;

namespace Esp32_Control.ViewModels;

public partial class DeviceSettingViewModel : ViewModelBase
{    
    [ObservableProperty]
    private int? numberMode = 1;

    private float? _setTemp = 1.0f;
    public float? SetTemp
    {
        get => _setTemp;
        set
        {
            if (value == _setTemp) return;
            _setTemp = value;
            OnPropertyChanged(nameof(SetTemp));
            _events.Publish(new SettingChangedEvent("SetTemp", _setTemp ?? 1.0f));
        }
    }
    private int? _fan1Speed = 1;
    public int? Fan1Speed
    {
        get => _fan1Speed;
        set
        {
            if (value == _fan1Speed) return;
            _fan1Speed = value;
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
            _fan2Speed = value;
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
            _tecPower = value;
            OnPropertyChanged(nameof(TecPower));
            _events.Publish(new SettingChangedEvent("TecPower", _tecPower ?? 10));
        }
    }
    private int? _mode = 1;
    public int? Mode
    {
        get => _mode;
        set
        {
            if (value == _mode) return;
            _mode = value;
            OnPropertyChanged(nameof(Mode));
            _events.Publish(new SettingChangedEvent("Mode", _mode ?? 1));
        }
    }
    private int? _modeIndex = 1;
    public int? ModeIndex
    {
        get => _modeIndex;
        set
        {
            if (value == _modeIndex) return;
            _modeIndex = value;
            OnPropertyChanged(nameof(ModeIndex));
            _events.Publish(new SettingChangedEvent("ModeIndex", _modeIndex ?? 1));
        }
    }

    public DeviceSettingViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {
        if (_store.SelectedDevice?.deviceStatus == null) return;

        _subscriptions.Add(_events.Subscribe<DeviceStatusChangedEvent>( evt =>
        {
            SetTemp = _store.SelectedDevice.deviceStatus.SetTemp;
            Fan1Speed = _store.SelectedDevice.deviceStatus.Fan1Speed;
            Fan2Speed = _store.SelectedDevice.deviceStatus.Fan2Speed;
            TecPower = _store.SelectedDevice.deviceStatus.TecPower;
            Mode = _store.SelectedDevice.deviceStatus.Mode;
            ModeIndex = _store.SelectedDevice.deviceStatus.ModeIndex;
            
            if (NumberMode != _store.SelectedDevice.deviceStatus.NumberMode)
                NumberMode = _store.SelectedDevice.deviceStatus.NumberMode;
        }));
    }
}
