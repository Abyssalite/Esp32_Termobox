using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.Input;
using Esp32_Control.Events;
using Microsoft.Extensions.DependencyInjection;
using Websocket.Client;

namespace Esp32_Control.ViewModels;

public partial class DeviceViewModel : ViewModelBase
{    
    private CancellationTokenSource? _delayToken;

    private readonly ITabView _tabview;
    public ITabView TabView => _tabview;
    public ICommand? BackCommand { get; }
    private WebsocketClient? _wsClient;
    
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

    public DeviceViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        ITabView tabs
    ):base(store, navigator, events)
    {
        _tabview = tabs;
        SelectedDevice = _store.SelectedDevice;
        BackCommand = new AsyncRelayCommand(BackAsync);
        if (SelectedDevice == null) return;
        
        Status = SelectedDevice.Status;
              
        _subscriptions.Add(_events.Subscribe<TabChangedEvent>(async evt =>
        {
            await _tabview.switchMainTab(evt.index);
        }));

        _subscriptions.Add(_events.Subscribe<SettingChangedEvent>(evt =>
        {
            if (SelectedDevice.deviceStatus != null)
            {
                _delayToken?.Cancel();
                _delayToken = new CancellationTokenSource();

                var token = _delayToken.Token;
                Task.Delay(200, token).ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    if (_wsClient?.IsStarted == true)
                        _wsClient.Send($"{evt.name}:{evt.value}");
                });
            }
        }));
        
        _subscriptions.Add(_events.Subscribe<LayoutChangedEvent>(async evt =>
        {
            if (evt.mode == 2)
            {
                await _tabview.switchMainTab(1);
                await _tabview.switchSecondaryTab(2);
            }
            if (evt.mode == 3)
            {
                await _tabview.switchSecondaryTab(3);
            }
        }));

        _ = InitializeAsync();
        _ = ConnectAsync(SelectedDevice);
    }

    public async Task InitializeAsync()
    {        
        var status = App.Services?.GetRequiredService<DeviceStatusViewModel>();
        var setting = App.Services?.GetRequiredService<DeviceSettingViewModel>();
        var tabbar = App.Services?.GetRequiredService<TabBarViewModel>();

        await _tabview.addTab(status);
        await _tabview.addTab(setting);
        await _tabview.addTab(tabbar);

        await _tabview.switchMainTab(1);
        await _tabview.switchSecondaryTab(3);
    }

    public async Task ConnectAsync(Device device)
    {
        var url = new Uri($"ws://{device.Address}/ws");

        _wsClient = new WebsocketClient(url)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(10),
            ErrorReconnectTimeout = TimeSpan.FromSeconds(10)
        };

        _wsClient.MessageReceived.Subscribe(msg =>
        {
            try
            {
                var status = System.Text.Json.JsonSerializer.Deserialize<DeviceStatus>(msg.Text ?? "");

                if (status != null)
                {
                    _store.StoreUpdateDeviceStatus(status); 
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
