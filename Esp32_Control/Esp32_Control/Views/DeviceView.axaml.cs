using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia_EventHub;
using Esp32_Control.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Esp32_Control.Views;

public partial class DeviceView : UserControl
{
    private CancellationTokenSource? _resizeToken;
    private readonly IEventHub? _events;

    public DeviceView()
    {
        InitializeComponent();
        _events = App.Services?.GetRequiredService<IEventHub>();

        this.LayoutUpdated += (_, __) =>
        {
            _resizeToken?.Cancel();
            _resizeToken = new CancellationTokenSource();

            var token = _resizeToken.Token;
            Task.Delay(10, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                Dispatcher.UIThread.InvokeAsync(AdjustStackOrientation);
            });
        };
    }

    private void AdjustStackOrientation()
    {
        if (_events == null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var width = topLevel.Bounds.Width;

        if (width < 800)
        {
            DeviceStatusContent.Margin = new Thickness(80, 0, 80, 0);
            DeviceSettingContent.Margin = new Thickness(0, 0, 0, 0);

            DeviceStatusGrid.RowDefinitions = new RowDefinitions("*,Auto");
            DeviceStatusGrid.ColumnDefinitions = new ColumnDefinitions("*");
            Grid.SetRow(DeviceSettingContent, 1);
            Grid.SetColumn(DeviceSettingContent, 0);
            _events.Publish(new LayoutChangedEvent(3));
        }

        else if (width > 800)
        {
            DeviceStatusContent.Margin = new Thickness(100, 0, 20, 0);
            DeviceSettingContent.Margin = new Thickness(20 ,0 ,100, 0);

            DeviceStatusGrid.RowDefinitions = new RowDefinitions("*");
            DeviceStatusGrid.ColumnDefinitions = new ColumnDefinitions("*,*");
            Grid.SetRow(DeviceSettingContent, 0);
            Grid.SetColumn(DeviceSettingContent, 1);
            _events.Publish(new LayoutChangedEvent(2));
        }
    }
}