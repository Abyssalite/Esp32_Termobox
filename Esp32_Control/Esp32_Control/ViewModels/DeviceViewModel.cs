using System.Threading;
using System.Threading.Tasks;
using Avalonia_EventHub;
using Avalonia_Navigation;
using Esp32_Control.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Esp32_Control.ViewModels;

public partial class DeviceViewModel : ViewModelBase, IHandleBackNavigation
{    
    private CancellationTokenSource? _delayToken;

    private readonly ITabView _tabview;
    public ITabView TabView => _tabview;
    private readonly IDeviceConnectionService _connection;

    public Device? SelectedDevice { get; }
    public string? Status { set; get; }

    public DeviceViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        ITabView tabs,
        IDeviceConnectionService connection
    ):base(store, navigator, events)
    {        
        _connection = connection;
        _tabview = tabs;
        SelectedDevice = _store.SelectedDevice;
        if (SelectedDevice == null) return;
        
        _connection.StatusReceived += OnStatusReceived;
        _connection.ConnectionStatusChanged += (_,status)=> { Status=status; };

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
                Task.Delay(50, token).ContinueWith(async t =>
                {
                    if (t.IsCanceled) return;

                    await SendSetting(evt.name, evt.value);
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
        _ = ConnectAsync();
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

    private async Task ConnectAsync()
    {
        if (SelectedDevice == null)
            return;
        await _connection.ConnectAsync(SelectedDevice);
    }

    private void OnStatusReceived(object? sender, DeviceStatus status)
    {
        _store.StoreUpdateDeviceStatus(status);
    }

    public async Task SendSetting(string name, float value)
    {
        _connection.Send($"{name}:{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }

    private async Task ClearAsync()
    {
        await _connection.DisconnectAsync();

        _store.SelectedDevice = null;
        Status = null;
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
