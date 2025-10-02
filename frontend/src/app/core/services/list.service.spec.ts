import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ListService, ListDto, CreateListDto, UpdateListDto } from './list.service';
import { environment } from '../../../environments/environment';

describe('ListService', () => {
  let service: ListService;
  let httpMock: HttpTestingController;

  const mockList: ListDto = {
    id: 'list1',
    title: 'Test List',
    boardId: 'board1',
    order: 1,
    items: [],
    notes: 'Test notes',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ListService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ListService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getListsByBoardId', () => {
    it('should retrieve lists for a board', (done) => {
      const boardId = 'board1';
      const mockLists: ListDto[] = [mockList];

      service.getListsByBoardId(boardId).subscribe({
        next: (lists) => {
          expect(lists).toEqual(mockLists);
          expect(lists.length).toBe(1);
          expect(lists[0].boardId).toBe(boardId);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/board/${boardId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockLists);
    });

    it('should handle empty board lists', (done) => {
      const boardId = 'board2';

      service.getListsByBoardId(boardId).subscribe({
        next: (lists) => {
          expect(lists).toEqual([]);
          expect(lists.length).toBe(0);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/board/${boardId}`);
      req.flush([]);
    });

    it('should handle error when retrieving board lists', (done) => {
      const boardId = 'board1';

      service.getListsByBoardId(boardId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/board/${boardId}`);
      req.flush({ message: 'Board not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('createList', () => {
    it('should create a new list', (done) => {
      const createDto: CreateListDto = {
        title: 'New List',
        boardId: 'board1',
        order: 1,
        items: [],
        notes: 'New notes'
      };

      service.createList(createDto).subscribe({
        next: (list) => {
          expect(list).toEqual(mockList);
          expect(list.title).toBe(mockList.title);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockList);
    });

    it('should create a list without optional fields', (done) => {
      const createDto: CreateListDto = {
        title: 'Minimal List',
        boardId: 'board1',
        order: 2
      };

      const minimalList: ListDto = {
        ...mockList,
        title: 'Minimal List',
        order: 2,
        items: [],
        notes: undefined
      };

      service.createList(createDto).subscribe({
        next: (list) => {
          expect(list).toEqual(minimalList);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists`);
      expect(req.request.body).toEqual(createDto);
      req.flush(minimalList);
    });

    it('should handle error when creating list', (done) => {
      const createDto: CreateListDto = {
        title: 'Invalid List',
        boardId: 'invalid-board',
        order: 1
      };

      service.createList(createDto).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists`);
      req.flush({ message: 'Invalid board ID' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('updateList', () => {
    it('should update an existing list', (done) => {
      const listId = 'list1';
      const updateDto: UpdateListDto = {
        title: 'Updated List',
        order: 2,
        items: [{ id: 'item1', text: 'Item 1', completed: false }]
      };

      const updatedList: ListDto = {
        ...mockList,
        ...updateDto
      };

      service.updateList(listId, updateDto).subscribe({
        next: (list) => {
          expect(list).toEqual(updatedList);
          expect(list.title).toBe('Updated List');
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/${listId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedList);
    });

    it('should handle error when updating list', (done) => {
      const listId = 'nonexistent';
      const updateDto: UpdateListDto = {
        title: 'Updated List',
        order: 2
      };

      service.updateList(listId, updateDto).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/${listId}`);
      req.flush({ message: 'List not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('deleteList', () => {
    it('should delete a list', (done) => {
      const listId = 'list1';

      service.deleteList(listId).subscribe({
        next: () => {
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/${listId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle error when deleting list', (done) => {
      const listId = 'list1';

      service.deleteList(listId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/${listId}`);
      req.flush({ message: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });
    });

    it('should handle deleting non-existent list', (done) => {
      const listId = 'nonexistent';

      service.deleteList(listId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/lists/${listId}`);
      req.flush({ message: 'List not found' }, { status: 404, statusText: 'Not Found' });
    });
  });
});
