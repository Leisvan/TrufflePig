using System;

namespace TrufflePig.Models;

public class NavigationRecord(string url, string favIconUri, string title, DateTime lastVisited, int visitCount)
{
    public string FavIconUri { get; set; } = favIconUri;

    public DateTime LastVisited { get; set; } = lastVisited;

    public string Title { get; set; } = title;

    public string Url { get; set; } = url;

    public int VisitCount { get; set; } = visitCount;

    public override string ToString() => Url;
}