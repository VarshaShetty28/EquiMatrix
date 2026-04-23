import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '@core/services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit {
  notifications: Notification[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private notificationService: NotificationService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.notificationService.getNotifications().subscribe({
      next: (data: Notification[]) => {
        this.notifications = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Error loading notifications:', err);
        this.errorMessage = 'Failed to load notifications';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  markAsRead(notificationId: number): void {
    const notification = this.notifications.find(n => n.notificationId === notificationId);
    if (!notification) return;

    // Store the original state in case we need to revert
    const originalState = notification.is_Read;

    // Update UI immediately
    notification.is_Read = true;
    this.cdr.markForCheck();

    // Call API to persist the change
    this.notificationService.markAsRead(notificationId).subscribe({
      next: () => {
        // Success - notification is already marked as read in UI
        console.log(`Notification ${notificationId} marked as read successfully`);
      },
      error: (err: any) => {
        console.error('Error marking notification as read:', err);
        // Revert the change if API call fails
        notification.is_Read = originalState;
        this.cdr.markForCheck();
        
        // Get detailed error message
        let errorMessage = 'Failed to update notification status. Please try again.';
        if (err.error && typeof err.error === 'object') {
          errorMessage = err.error.message || err.error.error || JSON.stringify(err.error);
        } else if (err.message) {
          errorMessage = err.message;
        }
        
        alert(errorMessage);
      }
    });
  }
}
