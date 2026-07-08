using System.Collections.ObjectModel;

namespace Esp32_Control.Events;

public sealed record SelectedDeviceChangedEvent(Device? device);


