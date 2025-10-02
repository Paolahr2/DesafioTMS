// HTTP Repository Implementations - Implementaciones concretas de los repositorios
// Usan HttpClient de Angular para comunicación con el backend

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { User, Board, List, Card, Comment } from '../../models/entities';
import { IBoardRepository, IListRepository, ICardRepository, IUserRepository, ICommentRepository } from '../../models/repositories';
import { API_CONFIG } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class HttpUserRepository implements IUserRepository {
  private apiUrl = API_CONFIG.baseUrl + API_CONFIG.endpoints.users;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<User | null> {
    return this.http.get<User>(`${this.apiUrl}/${id}`).pipe(
      map(user => user ?? null)
    ).toPromise() as Promise<User | null>;
  }

  findByEmail(email: string): Promise<User | null> {
    const params = new HttpParams().set('email', email);
    return this.http.get<User[]>(this.apiUrl, { params }).pipe(
      map(users => users?.length > 0 ? users[0] : null)
    ).toPromise() as Promise<User | null>;
  }

  findByUsername(username: string): Promise<User | null> {
    const params = new HttpParams().set('username', username);
    return this.http.get<User[]>(this.apiUrl, { params }).pipe(
      map(users => users?.length > 0 ? users[0] : null)
    ).toPromise() as Promise<User | null>;
  }

  create(user: Omit<User, 'id' | 'createdAt' | 'updatedAt'>): Promise<User> {
    return this.http.post<User>(this.apiUrl, user).toPromise() as Promise<User>;
  }

  update(id: string, user: Partial<User>): Promise<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, user).toPromise() as Promise<User>;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as Promise<void>;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpBoardRepository implements IBoardRepository {
  private apiUrl = API_CONFIG.baseUrl + API_CONFIG.endpoints.boards;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<Board | null> {
    return this.http.get<Board>(`${this.apiUrl}/${id}`).pipe(
      map(board => board ?? null)
    ).toPromise() as Promise<Board | null>;
  }

  findByOwnerId(ownerId: string): Promise<Board[]> {
    const params = new HttpParams().set('ownerId', ownerId);
    return this.http.get<Board[]>(this.apiUrl, { params }).toPromise() as Promise<Board[]>;
  }

  findByMemberId(memberId: string): Promise<Board[]> {
    const params = new HttpParams().set('memberId', memberId);
    return this.http.get<Board[]>(this.apiUrl, { params }).toPromise() as Promise<Board[]>;
  }

  findPublicBoards(): Promise<Board[]> {
    const params = new HttpParams().set('isPublic', 'true');
    return this.http.get<Board[]>(this.apiUrl, { params }).toPromise() as Promise<Board[]>;
  }

  create(board: Omit<Board, 'id' | 'createdAt' | 'updatedAt'>): Promise<Board> {
    return this.http.post<Board>(this.apiUrl, board).toPromise() as Promise<Board>;
  }

  update(id: string, board: Partial<Board>): Promise<Board> {
    return this.http.put<Board>(`${this.apiUrl}/${id}`, board).toPromise() as Promise<Board>;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as Promise<void>;
  }

  addMember(boardId: string, memberId: string): Promise<void> {
    return this.http.post<void>(`${this.apiUrl}/${boardId}/members`, { memberId }).toPromise() as Promise<void>;
  }

  removeMember(boardId: string, memberId: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${boardId}/members/${memberId}`).toPromise() as Promise<void>;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpListRepository implements IListRepository {
  private apiUrl = API_CONFIG.baseUrl + API_CONFIG.endpoints.lists;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<List | null> {
    return this.http.get<List>(`${this.apiUrl}/${id}`).pipe(
      map(list => list ?? null)
    ).toPromise() as Promise<List | null>;
  }

  findByBoardId(boardId: string): Promise<List[]> {
    const params = new HttpParams().set('boardId', boardId);
    return this.http.get<List[]>(this.apiUrl, { params }).toPromise() as Promise<List[]>;
  }

  create(list: Omit<List, 'id' | 'createdAt' | 'updatedAt'>): Promise<List> {
    return this.http.post<List>(this.apiUrl, list).toPromise() as Promise<List>;
  }

  update(id: string, list: Partial<List>): Promise<List> {
    return this.http.put<List>(`${this.apiUrl}/${id}`, list).toPromise() as Promise<List>;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as Promise<void>;
  }

  updatePositions(boardId: string, positions: { id: string; position: number }[]): Promise<void> {
    return this.http.put<void>(`${this.apiUrl}/positions`, { boardId, positions }).toPromise() as Promise<void>;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpCardRepository implements ICardRepository {
  private apiUrl = API_CONFIG.baseUrl + API_CONFIG.endpoints.cards;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<Card | null> {
    return this.http.get<Card>(`${this.apiUrl}/${id}`).pipe(
      map(card => card ?? null)
    ).toPromise() as Promise<Card | null>;
  }

  findByListId(listId: string): Promise<Card[]> {
    const params = new HttpParams().set('listId', listId);
    return this.http.get<Card[]>(this.apiUrl, { params }).toPromise() as Promise<Card[]>;
  }

  findByBoardId(boardId: string): Promise<Card[]> {
    const params = new HttpParams().set('boardId', boardId);
    return this.http.get<Card[]>(this.apiUrl, { params }).toPromise() as Promise<Card[]>;
  }

  create(card: Omit<Card, 'id' | 'createdAt' | 'updatedAt'>): Promise<Card> {
    return this.http.post<Card>(this.apiUrl, card).toPromise() as Promise<Card>;
  }

  update(id: string, card: Partial<Card>): Promise<Card> {
    return this.http.put<Card>(`${this.apiUrl}/${id}`, card).toPromise() as Promise<Card>;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as Promise<void>;
  }

  updatePositions(listId: string, positions: { id: string; position: number }[]): Promise<void> {
    return this.http.put<void>(`${this.apiUrl}/positions`, { listId, positions }).toPromise() as Promise<void>;
  }

  moveToList(cardId: string, newListId: string, position: number): Promise<void> {
    return this.http.put<void>(`${this.apiUrl}/${cardId}/move`, { newListId, position }).toPromise() as Promise<void>;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpCommentRepository implements ICommentRepository {
  private apiUrl = API_CONFIG.baseUrl + API_CONFIG.endpoints.comments;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<Comment | null> {
    return this.http.get<Comment>(`${this.apiUrl}/${id}`).pipe(
      map(comment => comment ?? null)
    ).toPromise() as Promise<Comment | null>;
  }

  findByCardId(cardId: string): Promise<Comment[]> {
    const params = new HttpParams().set('cardId', cardId);
    return this.http.get<Comment[]>(this.apiUrl, { params }).toPromise() as Promise<Comment[]>;
  }

  create(comment: Omit<Comment, 'id' | 'createdAt' | 'updatedAt'>): Promise<Comment> {
    return this.http.post<Comment>(this.apiUrl, comment).toPromise() as Promise<Comment>;
  }

  update(id: string, comment: Partial<Comment>): Promise<Comment> {
    return this.http.put<Comment>(`${this.apiUrl}/${id}`, comment).toPromise() as Promise<Comment>;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as Promise<void>;
  }
}




