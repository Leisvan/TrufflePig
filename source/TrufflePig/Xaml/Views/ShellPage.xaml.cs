using LCTWorks.WinUI.Extensions;
using LCTWorks.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using TrufflePig.ViewModels;

namespace TrufflePig.Xaml.Views;

public sealed partial class ShellPage : Page
{
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = MainFrame;
        TitleBarHelper.Extend(AppTitleBar, AppTitleBarText, "AppDisplayName".GetTextLocalized());
        InAppNotificationHelper.InfoBar = NotificationsBar;
    }

    public ShellViewModel ViewModel
    {
        get;
    }

    private void PageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        StartupCheckAsync();
        ViewModel.NavigateTo();
    }

    private async void StartupCheckAsync()
    {
        //This is the first moment where the XamlRoot isn't null
        var results = StartupHelper.Check();
        if (results == StartupCheckResult.FirstRun)
        {
            return;
        }
        if (results == StartupCheckResult.Updated)
        {
            //await _dialogService.ShowWhatsNewDialogAsync();
            return;
        }
        if (results == StartupCheckResult.RatingPrompt)
        {
            //Handle rating prompt here.
        }
    }
}