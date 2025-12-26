using LCTWorks.WinUI.Extensions;
using LCTWorks.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using Windows.UI.ViewManagement;

namespace TrufflePig;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherQueue _dispatcherQueue;

    private readonly UISettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetTextLocalized();

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _settings = new UISettings();
        _settings.ColorValuesChanged += SettingsColorValuesChanged;
    }

    private void SettingsColorValuesChanged(UISettings sender, object args)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}