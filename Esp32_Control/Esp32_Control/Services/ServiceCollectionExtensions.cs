using Esp32_Control.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<Store>();
        
        collection.AddTransient<MainViewModel>();
        collection.AddTransient<SelectViewModel>();
        collection.AddTransient<AddDeviceViewModel>();
        collection.AddTransient<DeviceViewModel>();
    }
}