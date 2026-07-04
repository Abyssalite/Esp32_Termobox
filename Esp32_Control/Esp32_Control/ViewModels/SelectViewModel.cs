
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia_Navigation;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Esp32_Control.ViewModels;

public partial class SelectViewModel : ViewModelBase
{    
    public ICommand AddDeviceCommand { get; }

    public SelectViewModel(
        Store store,
        INavigatorService navigator
    ):base(store, navigator)
    {
        AddDeviceCommand = new AsyncRelayCommand(addDeviceAsync);
    }

    async Task addDeviceAsync()
    {
        var vm = App.Services?.GetRequiredService<AddDeviceViewModel>();
        await _navigator.NavigateMain(vm);     
    }
}
