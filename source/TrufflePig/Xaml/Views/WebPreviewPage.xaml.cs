using Microsoft.UI.Xaml.Controls;
using TrufflePig.ViewModels;

namespace TrufflePig.Xaml.Views;

public sealed partial class WebPreviewPage : Page
{
    public WebPreviewPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<WebPreviewViewModel>();
        ViewModel.SetWebViewAsync(WebView);
    }

    public WebPreviewViewModel ViewModel
    {
        get;
    }
}