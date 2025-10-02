import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NotificationService, Notification } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NotificationService]
    });
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('show', () => {
    it('should add a notification', (done) => {
      const notification = {
        type: 'info' as const,
        title: 'Test',
        message: 'Test message'
      };

      service.show(notification);

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(1);
          expect(notifications[0].title).toBe('Test');
          expect(notifications[0].message).toBe('Test message');
          expect(notifications[0].type).toBe('info');
          expect(notifications[0].id).toBeDefined();
          expect(notifications[0].timestamp).toBeInstanceOf(Date);
          done();
        }
      });
    });

    it('should add multiple notifications', (done) => {
      service.show({ type: 'info', title: 'First', message: 'First message' });
      service.show({ type: 'success', title: 'Second', message: 'Second message' });

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(2);
          expect(notifications[0].title).toBe('First');
          expect(notifications[1].title).toBe('Second');
          done();
        }
      });
    });

    it('should auto-remove notification after duration', fakeAsync(() => {
      const notification = {
        type: 'info' as const,
        title: 'Auto Remove',
        message: 'Will be removed',
        duration: 1000
      };

      service.show(notification);

      let notificationCount = 0;
      service.notifications.subscribe({
        next: (notifications) => {
          notificationCount = notifications.length;
        }
      });

      expect(notificationCount).toBe(1);

      tick(1000);

      expect(notificationCount).toBe(0);
    }));

    it('should not auto-remove notification with duration 0', fakeAsync(() => {
      const notification = {
        type: 'info' as const,
        title: 'No Auto Remove',
        message: 'Will stay',
        duration: 0
      };

      service.show(notification);

      let notificationCount = 0;
      service.notifications.subscribe({
        next: (notifications) => {
          notificationCount = notifications.length;
        }
      });

      expect(notificationCount).toBe(1);

      tick(5000);

      expect(notificationCount).toBe(1);
    }));
  });

  describe('success', () => {
    it('should create a success notification', (done) => {
      service.success('Success Title', 'Success message');

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(1);
          expect(notifications[0].type).toBe('success');
          expect(notifications[0].title).toBe('Success Title');
          expect(notifications[0].message).toBe('Success message');
          done();
        }
      });
    });

    it('should create success notification with custom duration', (done) => {
      service.success('Success', 'Message', 3000);

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications[0].duration).toBe(3000);
          done();
        }
      });
    });
  });

  describe('error', () => {
    it('should create an error notification', (done) => {
      service.error('Error Title', 'Error message');

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(1);
          expect(notifications[0].type).toBe('error');
          expect(notifications[0].title).toBe('Error Title');
          expect(notifications[0].message).toBe('Error message');
          done();
        }
      });
    });
  });

  describe('warning', () => {
    it('should create a warning notification', (done) => {
      service.warning('Warning Title', 'Warning message');

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(1);
          expect(notifications[0].type).toBe('warning');
          expect(notifications[0].title).toBe('Warning Title');
          expect(notifications[0].message).toBe('Warning message');
          done();
        }
      });
    });
  });

  describe('info', () => {
    it('should create an info notification', (done) => {
      service.info('Info Title', 'Info message');

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(1);
          expect(notifications[0].type).toBe('info');
          expect(notifications[0].title).toBe('Info Title');
          expect(notifications[0].message).toBe('Info message');
          done();
        }
      });
    });

    it('should create info notification with action', (done) => {
      const action = { type: 'navigate' as const, data: { taskId: 'task1' } };
      service.info('Info', 'Message', undefined, action);

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications[0].action).toBeDefined();
          expect(notifications[0].action?.type).toBe('navigate');
          expect(notifications[0].action?.data.taskId).toBe('task1');
          done();
        }
      });
    });
  });

  describe('welcome', () => {
    it('should create a welcome notification with center position', (done) => {
      service.welcome('Welcome Title', 'Welcome message');

      service.notifications.subscribe({
        next: (notifications) => {
          if (notifications.length > 0) {
            expect(notifications.length).toBe(1);
            expect(notifications[0].type).toBe('welcome');
            expect(notifications[0].title).toBe('Welcome Title');
            expect(notifications[0].message).toBe('Welcome message');
            expect(notifications[0].position).toBe('center');
            expect(notifications[0].duration).toBe(5000);
            done();
          }
        }
      });
    });

    it('should create welcome notification with custom duration', (done) => {
      service.welcome('Welcome', 'Message', 3000);

      service.notifications.subscribe({
        next: (notifications) => {
          if (notifications.length > 0) {
            expect(notifications[0].duration).toBe(3000);
            done();
          }
        }
      });
    });
  });

  describe('remove', () => {
    it('should remove a specific notification by id', (done) => {
      service.show({ type: 'info', title: 'First', message: 'First message' });
      service.show({ type: 'info', title: 'Second', message: 'Second message' });

      let notificationId = '';
      
      service.notifications.subscribe({
        next: (notifications) => {
          if (notifications.length === 2) {
            notificationId = notifications[0].id;
            service.remove(notificationId);
          } else if (notifications.length === 1) {
            expect(notifications[0].title).toBe('Second');
            expect(notifications.every(n => n.id !== notificationId)).toBe(true);
            done();
          }
        }
      });
    });

    it('should not fail when removing non-existent notification', () => {
      service.show({ type: 'info', title: 'Test', message: 'Message' });
      
      expect(() => {
        service.remove('non-existent-id');
      }).not.toThrow();
    });
  });

  describe('clear', () => {
    it('should remove all notifications', (done) => {
      service.show({ type: 'info', title: 'First', message: 'First message' });
      service.show({ type: 'info', title: 'Second', message: 'Second message' });
      service.show({ type: 'info', title: 'Third', message: 'Third message' });

      service.clear();

      service.notifications.subscribe({
        next: (notifications) => {
          expect(notifications.length).toBe(0);
          done();
        }
      });
    });

    it('should not fail when clearing empty notifications', () => {
      expect(() => {
        service.clear();
      }).not.toThrow();
    });
  });

  describe('generateId', () => {
    it('should generate unique ids for different notifications', (done) => {
      service.show({ type: 'info', title: 'First', message: 'First' });
      service.show({ type: 'info', title: 'Second', message: 'Second' });
      service.show({ type: 'info', title: 'Third', message: 'Third' });

      service.notifications.subscribe({
        next: (notifications) => {
          const ids = notifications.map(n => n.id);
          const uniqueIds = new Set(ids);
          expect(uniqueIds.size).toBe(ids.length);
          done();
        }
      });
    });
  });
});
