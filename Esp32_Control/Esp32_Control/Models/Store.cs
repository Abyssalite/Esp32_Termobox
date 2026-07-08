
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia_EventHub;
using Esp32_Control.Events;

public class Store
{
    private readonly IEventHub _events;
    public required ObservableCollection<Device> DevicesLists { get; set; } = new();
    public Device? SelectedDevice;


    public Store(IEventHub events)
    {
        _events = events;
    }

    public void SelectDevice(Device? device)
    {
        if (SelectedDevice == device) return;

        SelectedDevice = device;
        _events.Publish(new SelectedDeviceChangedEvent(SelectedDevice));
    }

    public async Task<bool> StoreAddDevice(Device device)
    {
        var deviceName = DevicesLists.FirstOrDefault(d => d.Name == device.Name);
        if (deviceName != null) return true;

        DevicesLists.Add(device);

        await Helpers.SaveAsync(this);
        return false;
    }
}
