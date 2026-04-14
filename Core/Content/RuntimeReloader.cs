using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using SpringProject.Core.Debugging;

namespace SpringProject.Core;
					
public static class RuntimeReloader
{
	public static Action<object, FileSystemEventArgs> FileChangedEvent;
	public static Action<object, RenamedEventArgs> FileRenamedEvent; 

    const string PATH = "Data";
    static FileSystemWatcher _watcher = new FileSystemWatcher(PATH);

    static readonly Dictionary<string, DateTime> _lastReloadTime = new();
    const int DEBOUNCE_MS = 200;
	
	static RuntimeReloader()
	{
		_watcher.NotifyFilter = 
            NotifyFilters.Attributes | 
            NotifyFilters.CreationTime | 
            NotifyFilters.DirectoryName | 
            NotifyFilters.FileName | 
            NotifyFilters.LastAccess | 
            NotifyFilters.LastWrite| 
            NotifyFilters.Security| 
            NotifyFilters.Size;

		_watcher.Changed += OnChanged;
		_watcher.Renamed += OnRenamed;

        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        Debug.Log($"Runtime Reloader: Initialized!");
	}
	
	static void OnChanged(object sender, FileSystemEventArgs e)
	{
        if (!File.Exists(e.FullPath)) return; // skip directories
		if (e.ChangeType != WatcherChangeTypes.Changed) return;

        if (_lastReloadTime.TryGetValue(e.FullPath, out DateTime last) && (DateTime.Now - last).TotalMilliseconds < DEBOUNCE_MS) return;
        _lastReloadTime[e.FullPath] = DateTime.Now;

        Debug.Log($"Runtime Reloader: File '{e.FullPath}' was changed.");

        // wait for file to unlock before notifying listeners
        WaitForFile(e.FullPath);
        FileChangedEvent?.Invoke(sender, e);
	}
	
	static void OnRenamed(object sender, RenamedEventArgs e)
	{
        if (!File.Exists(e.FullPath)) return; // skip directories
		if (e.ChangeType != WatcherChangeTypes.Renamed) return;

        Debug.Log($"Runtime Reloader: File '{e.OldName}' was renamed to '{e.Name}'.");

        // wait for file to unlock before notifying listeners
        WaitForFile(e.FullPath);
        FileRenamedEvent?.Invoke(sender, e);
	}

    static void WaitForFile(string path, int maxAttempts = 10, int intervalInMs = 50)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            if (!IsFileLocked(path)) return;
            Thread.Sleep(intervalInMs);
        }

        Debug.Log($"Runtime Reloader: Timed out waiting for '{path}' to be released.");
    }

    static bool IsFileLocked(string filePath)
    {
        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return false;
            }
        }
        catch (IOException)
        {
            return true;
        }
    }
}