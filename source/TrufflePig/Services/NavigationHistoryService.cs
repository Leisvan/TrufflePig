using LCTWorks.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TrufflePig.Models;

namespace TrufflePig.Services;

public class NavigationHistoryService
{
    private const int MaxHistoryItems = 100;
    private const string NavigationHistoryKey = "NavigationHistory";
    private const int QueryResultLimit = 10;
    private List<NavigationRecord> _records = [];

    public NavigationHistoryService()
    {
        LoadHistory();
    }

    public void AddOrUpdateItem(string url, string favIconUri, string title = "")
    {
        var existingItem = _records.FirstOrDefault(item => item.Url.Equals(url, StringComparison.OrdinalIgnoreCase));

        if (existingItem != null)
        {
            existingItem.LastVisited = DateTime.Now;
            existingItem.VisitCount++;
            if (!string.IsNullOrEmpty(favIconUri))
            {
                existingItem.FavIconUri = favIconUri;
            }
            if (!string.IsNullOrEmpty(title))
            {
                existingItem.Title = title;
            }
        }
        else
        {
            _records.Add(new NavigationRecord(url, favIconUri, title, DateTime.Now, 1));

            if (_records.Count > MaxHistoryItems)
            {
                _records = [.. _records
                    .OrderByDescending(item => item.LastVisited)
                    .Take(MaxHistoryItems)];
            }
        }

        SaveHistory();
    }

    public List<NavigationRecord> GetAllHistory()
    {
        return [.. _records.OrderByDescending(item => item.LastVisited)];
    }

    public List<NavigationRecord> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [.. _records
                .OrderByDescending(item => item.LastVisited)
                .Take(QueryResultLimit)];
        }

        return [.. _records
            .Where(item =>
                item.Url.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.VisitCount)
            .ThenByDescending(item => item.LastVisited)
            .Take(QueryResultLimit)];
    }

    private void LoadHistory()
    {
        _records = LocalSettingsHelper.ReadSetting<List<NavigationRecord>>(NavigationHistoryKey) ?? [];
    }

    private void SaveHistory()
    {
        LocalSettingsHelper.SaveSetting(NavigationHistoryKey, _records);
    }
}