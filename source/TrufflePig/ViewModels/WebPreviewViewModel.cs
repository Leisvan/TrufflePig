using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCTWorks.Core.Extensions;
using LCTWorks.WinUI.Extensions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TrufflePig.Models;
using TrufflePig.Services;

namespace TrufflePig.ViewModels;

public partial class WebPreviewViewModel(NavigationHistoryService navigationHistoryService) : ObservableObject
{
    private const string DefaultFavIconUri = "ms-appx:///Assets/Images/DefaultSiteIcon.png";
    private readonly NavigationHistoryService _navigationHistoryService = navigationHistoryService;
    private string? _pendingNavigationUrl;
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

    public ObservableCollection<NavigationRecord> SuggestedRecords { get; } = [];

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

    public void UriTextBoxQuerySubmitted(AutoSuggestBox _, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(UriText))
        {
            return;
        }

        var input = UriText.Trim();

        if (args.ChosenSuggestion is NavigationRecord chosenItem)
        {
            _pendingNavigationUrl = chosenItem.Url;
            UriSource = new Uri(chosenItem.Url);
            return;
        }

        var uri = input.BuildValidUri();
        if (uri != null)
        {
            _pendingNavigationUrl = uri.ToString();
            UriSource = uri;
            return;
        }

        // Fallback to Google search
        var searchQuery = Uri.EscapeDataString(input);
        _pendingNavigationUrl = null;
        UriSource = new Uri($"https://www.google.com/search?q={searchQuery}");
    }

    public void UriTextBoxSuggestionChosen(AutoSuggestBox _, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is NavigationRecord item)
        {
            UriText = item.Url;
        }
    }

    public void UriTextBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = _navigationHistoryService.Search(sender.Text);
            // Check if suggestions have changed before updating
            if (!AreSuggestionsEqual(SuggestedRecords, suggestions))
            {
                SuggestedRecords.Clear();
                SuggestedRecords.AddRange(suggestions);
            }
        }
    }

    private static bool AreSuggestionsEqual(ObservableCollection<NavigationRecord> current, List<NavigationRecord> newSuggestions)
    {
        if (current.Count != newSuggestions.Count)
        {
            return false;
        }

        for (int i = 0; i < current.Count; i++)
        {
            if (current[i].Url != newSuggestions[i].Url)
            {
                return false;
            }
        }

        return true;
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
        if (_pendingNavigationUrl != null && args.IsSuccess)
        {
            var title = sender.DocumentTitle ?? string.Empty;
            var favIconUri = FavIconUri != DefaultFavIconUri ? FavIconUri : string.Empty;
            _navigationHistoryService.AddOrUpdateItem(_pendingNavigationUrl, favIconUri, title);
            _pendingNavigationUrl = null;
        }
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
            _pendingNavigationUrl = null;
            _webView.CoreWebView2.GoBack();
        }
    }

    [RelayCommand]
    private void GoForward()
    {
        if (_webView?.CoreWebView2 != null && _webView.CoreWebView2.CanGoForward)
        {
            _pendingNavigationUrl = null;
            _webView.CoreWebView2.GoForward();
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        if (_webView?.CoreWebView2 != null)
        {
            _pendingNavigationUrl = null;
            _webView.CoreWebView2.Reload();
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