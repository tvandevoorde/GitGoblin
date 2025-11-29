using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    
    [ObservableProperty]
    private ObservableCollection<Notification> _notifications = [];
    
    [ObservableProperty]
    private Notification? _selectedNotification;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _showUnreadOnly = true;

    public NotificationsViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var notifications = await _gitService.GetNotificationsAsync();
            
            if (ShowUnreadOnly)
            {
                notifications = notifications.Where(n => n.IsUnread);
            }
            
            Notifications = new ObservableCollection<Notification>(notifications);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load notifications: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(string notificationId)
    {
        try
        {
            await _gitService.MarkNotificationAsReadAsync(notificationId);
            
            var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsUnread = false;
                if (ShowUnreadOnly)
                {
                    Notifications.Remove(notification);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to mark notification as read: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadNotificationsAsync();
    }

    partial void OnShowUnreadOnlyChanged(bool value)
    {
        _ = LoadNotificationsAsync();
    }
}
