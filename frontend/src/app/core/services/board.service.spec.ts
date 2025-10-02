import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BoardService } from './board.service';
import { Board } from '@core/models/entities';

describe('BoardService', () => {
  let service: BoardService;
  let httpMock: HttpTestingController;
  const apiUrl = 'http://localhost:5003/api/boards';

  const mockBoard: Board = {
    id: 'board123',
    title: 'Test Board',
    description: 'Test Description',
    ownerId: 'user123',
    isPublic: false,
    isArchived: false,
    columns: [],
    memberIds: ['user123'],
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockBoards: Board[] = [
    mockBoard,
    {
      id: 'board456',
      title: 'Another Board',
      description: 'Another Description',
      ownerId: 'user123',
      isPublic: true,
      isArchived: false,
      columns: [],
      memberIds: ['user123', 'user456'],
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        BoardService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(BoardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getUserBoards', () => {
    it('should return an array of boards', (done) => {
      service.getUserBoards().subscribe(boards => {
        expect(boards).toEqual(mockBoards);
        expect(boards.length).toBe(2);
        done();
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockBoards);
    });

    it('should handle error when fetching boards', (done) => {
      service.getUserBoards().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
          done();
        }
      });

      const req = httpMock.expectOne(apiUrl);
      req.flush({ message: 'Server error' }, { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('getBoardById', () => {
    it('should return a single board', (done) => {
      const boardId = 'board123';

      service.getBoardById(boardId).subscribe(board => {
        expect(board).toEqual(mockBoard);
        expect(board.id).toBe(boardId);
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBoard);
    });

    it('should handle board not found error', (done) => {
      const boardId = 'nonexistent';

      service.getBoardById(boardId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      req.flush({ message: 'Board not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('createBoard', () => {
    it('should create a new board', (done) => {
      const newBoard: Partial<Board> = {
        title: 'New Board',
        description: 'New Description',
        isPublic: false
      };

      service.createBoard(newBoard).subscribe(board => {
        expect(board).toEqual(mockBoard);
        expect(board.title).toBe(mockBoard.title);
        done();
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newBoard);
      req.flush(mockBoard);
    });

    it('should handle validation error when creating board', (done) => {
      const invalidBoard: Partial<Board> = {
        title: '' // Invalid: empty title
      };

      service.createBoard(invalidBoard).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne(apiUrl);
      req.flush({ message: 'Title is required' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('updateBoard', () => {
    it('should update an existing board', (done) => {
      const boardId = 'board123';
      const updateData: Partial<Board> = {
        title: 'Updated Title',
        description: 'Updated Description'
      };

      const updatedBoard = { ...mockBoard, ...updateData };

      service.updateBoard(boardId, updateData).subscribe(board => {
        expect(board.title).toBe('Updated Title');
        expect(board.description).toBe('Updated Description');
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush(updatedBoard);
    });

    it('should handle unauthorized error when updating board', (done) => {
      const boardId = 'board123';
      const updateData: Partial<Board> = {
        title: 'Updated Title'
      };

      service.updateBoard(boardId, updateData).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      req.flush({ message: 'Unauthorized' }, { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('deleteBoard', () => {
    it('should delete a board', (done) => {
      const boardId = 'board123';

      service.deleteBoard(boardId).subscribe(() => {
        expect().nothing();
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle error when deleting board', (done) => {
      const boardId = 'board123';

      service.deleteBoard(boardId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/${boardId}`);
      req.flush({ message: 'Unauthorized' }, { status: 403, statusText: 'Forbidden' });
    });
  });
});
