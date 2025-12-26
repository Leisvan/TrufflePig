using TrufflePig.Helpers;
using TrufflePig.Models;

namespace TrufflePig.Xaml;

public static class Adapters
{
    public static string BuildName => AppHelper.BuildName();

    public static bool IsPreviewBuild => AppHelper.IsInternalBuild();

    public static string GetSecurityStateGlyph(SecurityState state)
    {
        return state switch
        {
            SecurityState.Unknown => "\uE9CE",
            SecurityState.Secure => "\uEA18",
            SecurityState.Insecure => "\uE730",
            SecurityState.CertificateError => "\uE783",
            _ => ""
        };
    }
}