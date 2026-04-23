import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '@core/services/notification.service';

@Component({
  selector: 'app-employee-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './enotifications.component.html'
})
export class EmployeeNotificationsComponent implements OnInit {
  notifications: Notification[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.notificationService.getMyNotifications().subscribe({
      next: (data: Notification[]) => {
        this.notifications = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading notifications:', err);
        this.errorMessage = 'Failed to load notifications';
        this.isLoading = false;
      }
    });
  }

  markAsRead(notification: Notification): void {
    notification.is_Read = true;

    this.notificationService.markAsRead(notification.notificationId).subscribe({
      next: () => {
        // Success - notification is already marked as read in UI
      },
      error: (err: any) => {
        console.error('Error marking notification as read:', err);
        // Revert the change if API call fails
        notification.is_Read = false;
      }
    });
  }
}
