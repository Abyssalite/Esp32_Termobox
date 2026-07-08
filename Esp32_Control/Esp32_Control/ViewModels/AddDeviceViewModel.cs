using Avalonia_Navigation;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia_EventHub;

namespace Esp32_Control.ViewModels;

public partial class AddDeviceViewModel : ViewModelBase
{    
    public string? Address { get; set; }
    public string? Name { get; set; }

    public ICommand? BackCommand { get; }
    public ICommand? AddDeviceCommand { get; }

    public AddDeviceViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events
    ):base(store, navigator, events)
    {
        BackCommand = new AsyncRelayCommand(ClearAsync);
        AddDeviceCommand = new AsyncRelayCommand(AddDevice);
    }

    private async Task ClearAsync()
    {
        Name = string.Empty;
        OnPropertyChanged(nameof(Name));
        Address = string.Empty;
        OnPropertyChanged(nameof(Address));

        await _navigator.OpenPrevious();
    }

    private async Task AddDevice()
    {
        string name = Helpers.InputOrDefault(Name, "");
        if (name == "")
            return;

        string address = Helpers.InputOrDefault(Address, "");
        if (!Helpers.IsValidIP(address))
            return;

        var device = new Device()
        {
            Name = name,
            Address = address
        };

        var result = await _store.StoreAddDevice(device);
        if (result) return;
        
        await ClearAsync();
    }
}
