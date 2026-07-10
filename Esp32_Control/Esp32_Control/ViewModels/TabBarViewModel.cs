
using System;
using System.Windows.Input;
using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.Input;
using Esp32_Control.Events;

namespace Esp32_Control.ViewModels;

public partial class TabBarViewModel : ViewModelBase
{
    public ICommand? StatusCommand { get; }
    public ICommand? SettingCommand { get; }

    public TabBarViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {
        StatusCommand = new RelayCommand<string>(switchTab);
        SettingCommand = new RelayCommand<string>(switchTab);
    }

    void switchTab(string? index)
    {
        if (index is not null)
            _events.Publish(new TabChangedEvent(Int32.Parse(index)));
    }
}