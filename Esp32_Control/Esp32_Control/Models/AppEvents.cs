using System.Collections.ObjectModel;

namespace Esp32_Control.Events;

public sealed record SelectedDeviceChangedEvent(Device? device);
public sealed record TabChangedEvent(int index);
public sealed record LayoutChangedEvent(int mode);
public sealed record DeviceStatusChangedEvent();
public sealed record SettingChangedEvent(string name, float value);


