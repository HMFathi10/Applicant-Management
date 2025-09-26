export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationOptions {
  title?: string;
  duration?: number;
  position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left' | 'top-center' | 'bottom-center';
}

class NotificationManager {
  private container: HTMLElement | null = null;
  private activeNotifications: HTMLElement[] = [];

  constructor() {
    this.createContainer();
  }

  private createContainer() {
    if (this.container) return;

    this.container = document.createElement('div');
    this.container.id = 'notification-container';
    this.container.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 400px;
      pointer-events: none;
    `;
    document.body.appendChild(this.container);
  }

  private createNotification(
    message: string,
    type: NotificationType,
    options: NotificationOptions = {}
  ): HTMLElement {
    const notification = document.createElement('div');
    const { title, duration = 3000, position = 'top-right' } = options;

    // Set position
    this.setPosition(position);

    // Icons for different types
    const icons = {
      success: '✅',
      error: '❌',
      warning: '⚠️',
      info: 'ℹ️'
    };

    // Colors for different types
    const colors = {
      success: {
        background: '#d4edda',
        border: '#c3e6cb',
        color: '#155724'
      },
      error: {
        background: '#f8d7da',
        border: '#f5c6cb',
        color: '#721c24'
      },
      warning: {
        background: '#fff3cd',
        border: '#ffeeba',
        color: '#856404'
      },
      info: {
        background: '#d1ecf1',
        border: '#bee5eb',
        color: '#0c5460'
      }
    };

    const colorScheme = colors[type];

    notification.style.cssText = `
      background-color: ${colorScheme.background};
      border: 1px solid ${colorScheme.border};
      color: ${colorScheme.color};
      padding: 12px 16px;
      border-radius: 4px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-size: 14px;
      line-height: 1.4;
      pointer-events: auto;
      transform: translateX(100%);
      transition: transform 0.3s ease-in-out;
      max-width: 100%;
    `;

    let content = '';
    if (title) {
      content += `<div style="font-weight: 600; margin-bottom: 4px;">${title}</div>`;
    }
    content += `<div style="display: flex; align-items: center; gap: 8px;">
      <span>${icons[type]}</span>
      <span>${message}</span>
    </div>`;

    notification.innerHTML = content;

    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification);
      }, duration);
    }

    return notification;
  }

  private setPosition(position: string) {
    if (!this.container) return;

    const positions = {
      'top-right': { top: '20px', right: '20px', bottom: 'auto', left: 'auto' },
      'top-left': { top: '20px', left: '20px', bottom: 'auto', right: 'auto' },
      'bottom-right': { bottom: '20px', right: '20px', top: 'auto', left: 'auto' },
      'bottom-left': { bottom: '20px', left: '20px', top: 'auto', right: 'auto' },
      'top-center': { top: '20px', left: '50%', transform: 'translateX(-50%)', bottom: 'auto', right: 'auto' },
      'bottom-center': { bottom: '20px', left: '50%', transform: 'translateX(-50%)', top: 'auto', right: 'auto' }
    };

    const pos = positions[position as keyof typeof positions];
    Object.assign(this.container.style, pos);
  }

  private removeNotification(notification: HTMLElement) {
    notification.style.transform = 'translateX(100%)';
    setTimeout(() => {
      notification.remove();
      const index = this.activeNotifications.indexOf(notification);
      if (index > -1) {
        this.activeNotifications.splice(index, 1);
      }
    }, 300);
  }

  private showNotification(message: string, type: NotificationType, options?: NotificationOptions) {
    if (!this.container) return;

    const notification = this.createNotification(message, type, options);
    this.container.appendChild(notification);
    this.activeNotifications.push(notification);

    // Animate in
    setTimeout(() => {
      notification.style.transform = 'translateX(0)';
    }, 10);

    return notification;
  }

  success(message: string, options?: NotificationOptions) {
    return this.showNotification(message, 'success', options);
  }

  error(message: string, options?: NotificationOptions) {
    return this.showNotification(message, 'error', { ...options, duration: 5000 });
  }

  warning(message: string, options?: NotificationOptions) {
    return this.showNotification(message, 'warning', options);
  }

  info(message: string, options?: NotificationOptions) {
    return this.showNotification(message, 'info', options);
  }

  clearAll() {
    this.activeNotifications.forEach(notification => {
      notification.remove();
    });
    this.activeNotifications = [];
  }
}

export const notificationManager = new NotificationManager();

// Convenience functions
export const showSuccess = (message: string, options?: NotificationOptions) => {
  return notificationManager.success(message, options);
};

export const showError = (message: string, options?: NotificationOptions) => {
  return notificationManager.error(message, options);
};

export const showWarning = (message: string, options?: NotificationOptions) => {
  return notificationManager.warning(message, options);
};

export const showInfo = (message: string, options?: NotificationOptions) => {
  return notificationManager.info(message, options);
};

export default notificationManager;