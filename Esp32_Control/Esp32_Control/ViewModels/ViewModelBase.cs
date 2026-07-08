using System;
using System.Collections.Generic;
using Avalonia_EventHub;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Esp32_Control.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected readonly Store _store;
    protected readonly INavigatorService _navigator;
    protected readonly IEventHub _events;
    protected readonly List<IDisposable> _subscriptions = new();

    protected ViewModelBase(
        Store store,
        INavigatorService navigator,
        IEventHub events

    ){
        _store = store;
        _navigator = navigator;
        _events = events;
    }
}
