using CommunityToolkit.Mvvm.ComponentModel;
using LCTWorks.WinUI.Navigation;

namespace TrufflePig.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public ShellViewModel(FrameNavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public FrameNavigationService NavigationService
    {
        get;
    }

    public void NavigateTo()
    {
        NavigationService.NavigateTo(typeof(WebPreviewViewModel).FullName!);
    }
}