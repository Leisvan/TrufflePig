namespace TrufflePig.Helpers;

public static class AppHelper
{
    public static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
    return false;
#endif
    }

    public static string BuildName()
    {
#if DEBUG
        return "Debug";
        //#elif RELEASEBETA
        //            return "Beta";
#else
            return "Release";
#endif
    }

    public static bool IsInternalBuild()
    {
#if DEBUG
        return true;
        //#elif RELEASEBETA
        //        return true;
#else
        return false;
#endif
    }
}