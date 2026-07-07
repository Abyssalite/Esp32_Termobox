
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia_EventHub;

public class Store
{
    private readonly IEventHub _events;
    public ObservableCollection<Device>? DevicesLists { get; private set; }

    public Store(IEventHub events)
    {
        _events = events;
    }

    public async Task StoreAddDevice()
    {
        
    }
}
