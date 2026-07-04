using System.Threading.Tasks;
using Avalonia_Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Esp32_Control.ViewModels;

public partial class MainViewModel : ViewModelBase
{    
    private readonly IViewHost _viewhost;
    public IViewHost ViewHost => _viewhost;

    public MainViewModel(
        Store store,
        IViewHost viewHost,
        INavigatorService navigator
    ):base(store, navigator)
    {
        _viewhost = viewHost;
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var vm = App.Services?.GetRequiredService<SelectViewModel>();
        await _navigator.NavigateMain(vm); 
    }
}
