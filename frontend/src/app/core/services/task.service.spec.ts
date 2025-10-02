import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TaskService } from './task.service';
import { NotificationService } from './notification.service';
import { TaskItem } from '../models/entities';
import { environment } from '../../../environments/environment';

describe('TaskService', () => {
  let service: TaskService;
  let httpMock: HttpTestingController;
  let notificationService: jasmine.SpyObj<NotificationService>;

  const mockTask: TaskItem = {
    id: 'task1',
    title: 'Test Task',
    description: 'Test Description',
    status: 0, // Pending
    priority: 'Medium',
    listId: 'list1',
    boardId: 'board1',
    assignedToId: 'user1',
    createdById: 'user1',
    createdByName: 'Test User',
    dueDate: new Date('2025-12-31'),
    tags: ['test'],
    position: 0,
    isCompleted: false,
    progressPercentage: 0,
    attachments: [],
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(() => {
    const notificationSpy = jasmine.createSpyObj('NotificationService', ['info', 'success', 'error']);

    TestBed.configureTestingModule({
      providers: [
        TaskService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: notificationSpy }
      ]
    });

    service = TestBed.inject(TaskService);
    httpMock = TestBed.inject(HttpTestingController);
    notificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getBoardTasks', () => {
    it('should retrieve tasks for a board', (done) => {
      const boardId = 'board1';
      const mockTasks: TaskItem[] = [mockTask];

      service.getBoardTasks(boardId).subscribe({
        next: (tasks) => {
          expect(tasks).toEqual(mockTasks);
          expect(tasks.length).toBe(1);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/board/${boardId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockTasks);
    });

    it('should handle error when retrieving board tasks', (done) => {
      const boardId = 'board1';

      service.getBoardTasks(boardId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/board/${boardId}`);
      req.flush({ message: 'Board not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('createTask', () => {
    it('should create a new task', (done) => {
      const newTask: Partial<TaskItem> = {
        title: 'New Task',
        description: 'New Description',
        listId: 'list1',
        boardId: 'board1'
      };

      service.createTask(newTask).subscribe({
        next: (response) => {
          expect(response).toEqual(mockTask);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newTask);
      req.flush(mockTask);
    });
  });

  describe('updateTask', () => {
    it('should update an existing task', (done) => {
      const taskId = 'task1';
      const updateData = {
        title: 'Updated Task',
        priority: 2
      };

      service.updateTask(taskId, updateData).subscribe({
        next: (response) => {
          expect(response).toBeTruthy();
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush({ ...mockTask, ...updateData });
    });
  });

  describe('changeTaskStatus', () => {
    it('should change task status to InProgress', (done) => {
      const taskId = 'task1';
      const status = 'InProgress';

      service.changeTaskStatus(taskId, status).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/status`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ Status: 1 }); // InProgress = 1
      req.flush({});
    });

    it('should change task status to Completed', (done) => {
      const taskId = 'task1';
      const status = 'Completed';

      service.changeTaskStatus(taskId, status).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/status`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ Status: 3 }); // Completed = 3
      req.flush({});
    });

    it('should change task status to Pending', (done) => {
      const taskId = 'task1';
      const status = 'Pending';

      service.changeTaskStatus(taskId, status).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/status`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ Status: 0 }); // Pending = 0
      req.flush({});
    });
  });

  describe('deleteTask', () => {
    it('should delete a task', (done) => {
      const taskId = 'task1';

      service.deleteTask(taskId).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should handle error when deleting task', (done) => {
      const taskId = 'task1';

      service.deleteTask(taskId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}`);
      req.flush({ message: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('assignTask', () => {
    it('should assign task to user and show notification', (done) => {
      const taskId = 'task1';
      const assignedToId = 'user2';
      const taskTitle = 'Test Task';

      service.assignTask(taskId, assignedToId, taskTitle).subscribe({
        next: () => {
          expect(notificationService.info).toHaveBeenCalledWith(
            'Tarea Asignada',
            `Se te ha asignado la tarea: ${taskTitle}`,
            undefined,
            { type: 'navigate', data: { taskId } }
          );
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/assign`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ AssignedToId: assignedToId });
      req.flush({});
    });

    it('should unassign task (null assignee) without notification', (done) => {
      const taskId = 'task1';
      const assignedToId = null;
      const taskTitle = 'Test Task';

      service.assignTask(taskId, assignedToId, taskTitle).subscribe({
        next: () => {
          expect(notificationService.info).not.toHaveBeenCalled();
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/assign`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ AssignedToId: null });
      req.flush({});
    });
  });

  describe('updateDueDate', () => {
    it('should update task due date', (done) => {
      const taskId = 'task1';
      const dueDate = new Date('2025-12-31');

      service.updateDueDate(taskId, dueDate).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/due-date`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body.dueDate).toEqual(dueDate);
      req.flush({});
    });

    it('should remove task due date (null)', (done) => {
      const taskId = 'task1';
      const dueDate = null;

      service.updateDueDate(taskId, dueDate).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}/due-date`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body.dueDate).toBeNull();
      req.flush({});
    });
  });

  describe('getTaskById', () => {
    it('should retrieve a task by id', (done) => {
      const taskId = 'task1';

      service.getTaskById(taskId).subscribe({
        next: (task) => {
          expect(task).toEqual(mockTask);
          expect(task.id).toBe(taskId);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockTask);
    });

    it('should handle error when task not found', (done) => {
      const taskId = 'nonexistent';

      service.getTaskById(taskId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.tasks}/${taskId}`);
      req.flush({ message: 'Task not found' }, { status: 404, statusText: 'Not Found' });
    });
  });
});
