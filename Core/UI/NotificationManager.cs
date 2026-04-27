using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpringProject.Core.UI;

public class Notification(string title)
{
    public readonly string Title = title;
    public double LifeTime = 5.0f;
}

public static class NotificationManager
{
    static readonly List<Notification> _notifications = new();

    public static Action<Notification> NotifyEvent;
    public static Action<Notification> RemoveEvent;

    static readonly List<Notification> _addQueue = new();
    static readonly List<Notification> _removeQueue = new();

    const int MAX = 8;

    public static void Notify(string title)
    {
        var notification = new Notification(title);
        _addQueue.Add(notification);
        NotifyEvent?.Invoke(notification);
    }

    public static void Update(GameTime gameTime)
    {
        double elapsed = gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var notification in _addQueue)
        {
            if (_notifications.Count >= MAX)
            {
                var oldest = _notifications[0];
                _notifications.RemoveAt(0);
                RemoveEvent?.Invoke(oldest);
            }
            _notifications.Add(notification);
        }

        foreach (var notification in _notifications)
        {
            notification.LifeTime -= elapsed;
            
            if (notification.LifeTime <= 0)
            {
                _removeQueue.Add(notification);
                RemoveEvent?.Invoke(notification);
            }
        }

        foreach (var notification in _removeQueue)
        {
            _notifications.Remove(notification);
        }

        _addQueue.Clear();
        _removeQueue.Clear();
    }
}