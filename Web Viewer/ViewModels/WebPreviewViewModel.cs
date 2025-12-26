using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCTWorks.Core.Extensions;
using LCTWorks.WinUI.Extensions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using WebViewer.Models;

namespace WebViewer.ViewModels;

public partial class WebPreviewViewModel : ObservableObject
{
    private const string DefaultFavIconUri = "ms-appx:///Assets/Images/DefaultSiteIcon.png";
    private WebView2? _webView;

    [ObservableProperty]
    public partial bool CanGoBack { get; set; }

    [ObservableProperty]
    public partial bool CanGoForward { get; set; }

    [ObservableProperty]
    public partial string FavIconUri { get; set; } = DefaultFavIconUri;

    public bool? IsMuted
    {
        get => _webView?.CoreWebView2?.IsMuted ?? false;
        set
        {
            if (value == null)
            {
                return;
            }
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.IsMuted = value.Value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavigationEnabled))]
    public partial bool IsNavigating { get; set; }

    public bool IsNavigationEnabled => !IsNavigating;

    [ObservableProperty]
    public partial bool IsPlayingAudio { get; set; }

    [ObservableProperty]
    public partial SecurityState SecurityState { get; set; } = SecurityState.Unknown;

    [ObservableProperty]
    public partial Uri UriSource { get; set; }

    [ObservableProperty]
    public partial string UriText { get; set; }

    public async void SetWebViewAsync(WebView2 webView)
    {
        _webView = webView;

        await _webView.EnsureCoreWebView2Async();

        if (_webView.CoreWebView2 != null)
        {
            _webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            _webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            _webView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            _webView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
            _webView.CoreWebView2.IsDocumentPlayingAudioChanged += CoreWebView2_IsDocumentPlayingAudioChanged;

            IsPlayingAudio = _webView.CoreWebView2.IsDocumentPlayingAudio;
        }
    }

    public void UriTextBoxQuerySubmitted(AutoSuggestBox _, AutoSuggestBoxQuerySubmittedEventArgs __)
    {
        if (string.IsNullOrWhiteSpace(UriText))
        {
            return;
        }

        var input = UriText.Trim();

        var uri = input.BuildValidUri();
        if (uri != null)
        {
            UriSource = uri;
            return;
        }

        // Fallback to Google search
        var searchQuery = Uri.EscapeDataString(input);
        UriSource = new Uri($"https://www.google.com/search?q={searchQuery}");
    }

    private void CoreWebView2_HistoryChanged(CoreWebView2 sender, object args)
    {
        UpdateNavigationState();
    }

    private void CoreWebView2_IsDocumentPlayingAudioChanged(CoreWebView2 sender, object args)
    {
        IsPlayingAudio = sender.IsDocumentPlayingAudio;
    }

    private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        IsNavigating = false;
        if (UriText != sender.Source)
        {
            UriText = sender.Source;
        }
        if (FavIconUri != sender.FaviconUri)
        {
            var uri = sender.FaviconUri.BuildValidUri();
            if (uri != null && uri.IsImageUri())
            {
                FavIconUri = sender.FaviconUri;
            }
        }
        UpdateNavigationState();
        UpdateSecurityState(args);
    }

    private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        IsNavigating = true;
    }

    private void CoreWebView2_SourceChanged(CoreWebView2 sender, CoreWebView2SourceChangedEventArgs args)
    {
        if (UriText != sender.Source)
        {
            UriText = sender.Source;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_webView?.CoreWebView2 != null && _webView.CoreWebView2.CanGoBack)
        {
            _webView.CoreWebView2.GoBack();
        }
    }

    [RelayCommand]
    private void GoForward()
    {
        if (_webView?.CoreWebView2 != null && _webView.CoreWebView2.CanGoForward)
        {
            _webView.CoreWebView2.GoForward();
        }
    }

    private void UpdateNavigationState()
    {
        if (_webView?.CoreWebView2 != null)
        {
            CanGoBack = _webView.CoreWebView2.CanGoBack;
            CanGoForward = _webView.CoreWebView2.CanGoForward;
        }
    }

    private void UpdateSecurityState(CoreWebView2NavigationCompletedEventArgs args)
    {
        if (_webView?.CoreWebView2 == null)
        {
            SecurityState = SecurityState.Unknown;
            return;
        }

        try
        {
            var uri = new Uri(_webView.CoreWebView2.Source);

            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                if (!args.IsSuccess && args.HasCertificateErrors())
                {
                    SecurityState = SecurityState.CertificateError;
                }
                else if (args.IsSuccess)
                {
                    SecurityState = SecurityState.Secure;
                }
                else
                {
                    SecurityState = SecurityState.Unknown;
                }
            }
            else if (uri.Scheme == Uri.UriSchemeHttp)
            {
                SecurityState = SecurityState.Insecure;
            }
            else
            {
                SecurityState = SecurityState.Unknown;
            }
        }
        catch
        {
            SecurityState = SecurityState.Unknown;
        }
    }
}