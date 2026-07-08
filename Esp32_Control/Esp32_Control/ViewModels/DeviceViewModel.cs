using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Websocket.Client;


namespace Esp32_Control.ViewModels;

public partial class DeviceViewModel : ViewModelBase
{    
    public Device? SelectedDevice { get; }
    private string? _status;
    public string? Status
    {
        get => _status;
        set
        {

            _status = value;

            SelectedDevice?.Status = _status;
            OnPropertyChanged(nameof(Status));
        }
    }
    [ObservableProperty]
    private DeviceStatus? deviceStatus = new();
    
    private WebsocketClient? _wsClient;
    public ICommand? BackCommand { get; }

    public DeviceViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {
        SelectedDevice = _store.SelectedDevice;
        BackCommand = new AsyncRelayCommand(BackAsync);

        if (SelectedDevice != null)
        {
            Status = SelectedDevice.Status;
            _ = ConnectAsync(SelectedDevice);

        }
    }

    public async Task ConnectAsync(Device device)
    {
        var url = new Uri($"ws://{device.Address}/ws");

        _wsClient = new WebsocketClient(url)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(10),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
        };

        _wsClient.MessageReceived.Subscribe(msg =>
        {
            try
            {
                var status = System.Text.Json.JsonSerializer.Deserialize<DeviceStatus>(msg.Text ?? "");
                if (status != null)
                {
                    DeviceStatus = status;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON Parse Error: " + ex.Message);
            }
        });

        _wsClient.ReconnectionHappened.Subscribe(info =>
        {
            Status = $"Status: {info.Type}";
        });

        await _wsClient.Start();
    }

    private async Task BackAsync()
    {
        _store.SelectedDevice = null;

        await _navigator.OpenPrevious();
    }
}
