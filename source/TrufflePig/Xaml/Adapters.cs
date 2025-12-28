using FluentIcons.Common;
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

    public static Icon GetSecurityStateIcon(SecurityState state)
    {
        return state switch
        {
            SecurityState.Unknown => Icon.ShieldQuestion,
            SecurityState.Secure => Icon.ShieldTask,
            SecurityState.Insecure => Icon.ShieldDismiss,
            SecurityState.CertificateError => Icon.ShieldDismiss,
            _ => Icon.ShieldQuestion,
        };
    }
}