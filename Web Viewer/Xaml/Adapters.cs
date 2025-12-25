using WebViewer.Helpers;

namespace WebViewer.Xaml;

public static class Adapters
{
    public static string BuildName => AppHelper.BuildName();

    public static bool IsPreviewBuild => AppHelper.IsInternalBuild();
}