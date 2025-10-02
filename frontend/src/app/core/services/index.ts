export { ListService } from './list.service';
export type { ListDto, CreateListDto, ListItem } from './list.service';

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { User, Board, List, Card, Comment } from '@core/models/entities';
import { IUserRepository, IBoardRepository, IListRepository, ICardRepository, ICommentRepository } from '@core/models/repositories';
import { environment } from '../../../environments/environment';

// Implementaciones directas simplificadas
@Injectable({
  providedIn: 'root'
})
export class HttpUserRepository implements IUserRepository {
  private readonly API_URL = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<User | null> {
    return this.http.get<any>(`${this.API_URL}/${id}`).pipe(
      map(user => user ? {
        id: user.id || '',
        email: user.email || '',
        username: user.username || '',
        fullName: user.fullName || '',
        avatar: user.avatar,
        createdAt: new Date(user.createdAt),
        updatedAt: new Date(user.updatedAt)
      } : null),
      catchError(() => of(null))
    ).toPromise() as any;
  }

  findByEmail(email: string): Promise<User | null> {
    return this.http.get<any>(`${this.API_URL}/email/${email}`).pipe(
      map(user => user ? {
        id: user.id || '',
        email: user.email || '',
        username: user.username || '',
        fullName: user.fullName || '',
        avatar: user.avatar,
        createdAt: new Date(user.createdAt),
        updatedAt: new Date(user.updatedAt)
      } : null),
      catchError(() => of(null))
    ).toPromise() as any;
  }

  findByUsername(username: string): Promise<User | null> {
    return this.http.get<any>(`${this.API_URL}/username/${username}`).pipe(
      map(user => user ? {
        id: user.id || '',
        email: user.email || '',
        username: user.username || '',
        fullName: user.fullName || '',
        avatar: user.avatar,
        createdAt: new Date(user.createdAt),
        updatedAt: new Date(user.updatedAt)
      } : null),
      catchError(() => of(null))
    ).toPromise() as any;
  }

  create(user: Omit<User, 'id' | 'createdAt' | 'updatedAt'>): Promise<User> {
    return this.http.post<any>(this.API_URL, user).pipe(
      map(user => ({
        id: user.id || '',
        email: user.email || '',
        username: user.username || '',
        fullName: user.fullName || '',
        avatar: user.avatar,
        createdAt: new Date(user.createdAt),
        updatedAt: new Date(user.updatedAt)
      }))
    ).toPromise() as any;
  }

  update(id: string, user: Partial<User>): Promise<User> {
    return this.http.put<any>(`${this.API_URL}/${id}`, user).pipe(
      map(user => ({
        id: user.id || '',
        email: user.email || '',
        username: user.username || '',
        fullName: user.fullName || '',
        avatar: user.avatar,
        createdAt: new Date(user.createdAt),
        updatedAt: new Date(user.updatedAt)
      }))
    ).toPromise() as any;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`).toPromise() as any;
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpBoardRepository implements IBoardRepository {
  private apiUrl = `${environment.apiUrl}/boards`;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<Board | null> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(board => board ? {
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      } : null),
      catchError(() => of(null))
    ).toPromise() as any;
  }

  findByOwnerId(ownerId: string): Promise<Board[]> {
    return this.http.get<any[]>(`${this.apiUrl}/owner/${ownerId}`).pipe(
      map(boards => boards.map(board => ({
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      }))),
      catchError(() => of([]))
    ).toPromise() as any;
  }

  findByMemberId(memberId: string): Promise<Board[]> {
    return this.http.get<any[]>(`${this.apiUrl}/member/${memberId}`).pipe(
      map(boards => boards.map(board => ({
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      }))),
      catchError(() => of([]))
    ).toPromise() as any;
  }

  findPublicBoards(): Promise<Board[]> {
    return this.http.get<any[]>(`${this.apiUrl}/public`).pipe(
      map(boards => boards.map(board => ({
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      }))),
      catchError(() => of([]))
    ).toPromise() as any;
  }

  create(board: Omit<Board, 'id' | 'createdAt' | 'updatedAt'>): Promise<Board> {
    return this.http.post<any>(this.apiUrl, board).pipe(
      map(board => ({
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      }))
    ).toPromise() as any;
  }

  update(id: string, board: Partial<Board>): Promise<Board> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, board).pipe(
      map(board => ({
        id: board.id || '',
        title: board.title || '',
        description: board.description,
        backgroundColor: board.backgroundColor || '#0079bf',
        backgroundImage: board.backgroundImage,
        ownerId: board.ownerId || '',
        members: board.members || [],
        isPublic: board.isPublic || false,
        createdAt: new Date(board.createdAt),
        updatedAt: new Date(board.updatedAt)
      }))
    ).toPromise() as any;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as any;
  }

  addMember(boardId: string, memberId: string): Promise<void> {
    return this.http.post<void>(`${this.apiUrl}/${boardId}/members`, { memberId }).toPromise() as any;
  }

  removeMember(boardId: string, memberId: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${boardId}/members/${memberId}`).toPromise() as any;
  }
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'current_user';

  constructor(private http: HttpClient) {}

  async loginWithParams(emailOrUsername: string, password: string): Promise<User> {
    try {
      const response = await this.http.post<any>(`${this.API_URL}/login`, {
        emailOrUsername,
        password
      }).toPromise();

      if (response && response.token && response.user) {
        // Guardar token y usuario en localStorage
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));

        return response.user;
      } else {
        throw new Error('Respuesta inválida del servidor');
      }
    } catch (error: any) {
      console.error('Error en login:', error);
      if (error.status === 401) {
        throw new Error('Credenciales inválidas');
      } else if (error.status === 0) {
        throw new Error('No se puede conectar al servidor. Verifica que el backend esté ejecutándose.');
      } else {
        throw new Error(error.error?.message || error.message || 'Error en el login');
      }
    }
  }

  async register(userData: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Promise<User> {
    try {
      const response = await this.http.post<any>(`${this.API_URL}/register`, userData).toPromise();

      if (response && response.token && response.user) {
        // Guardar token y usuario en localStorage
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));

        return response.user;
      } else {
        throw new Error('Respuesta inválida del servidor');
      }
    } catch (error: any) {
      console.error('Error en registro:', error);
      // Propagar el error original para que el componente pueda inspeccionar status y body
      if (error && (error.status !== undefined)) {
        throw error;
      }

      // Fallback a mensaje genérico
      if (error.status === 0) {
        throw new Error('No se puede conectar al servidor. Verifica que el backend esté ejecutándose.');
      }

      throw new Error(error?.message || 'Error en el registro');
    }
  }

  async getCurrentUser(): Promise<User | null> {
    try {
      const userData = localStorage.getItem(this.USER_KEY);
      const token = localStorage.getItem(this.TOKEN_KEY);

      if (userData && token) {
        const user = JSON.parse(userData);
        return user;
      }
      return null;
    } catch (error) {
      console.error('Error obteniendo usuario actual:', error);
      return null;
    }
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  forgotPassword(email: string) {
    return this.http.post(`${this.API_URL}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string) {
    return this.http.post(`${this.API_URL}/reset-password`, { token, newPassword });
  }
}

@Injectable({
  providedIn: 'root'
})
export class BoardService {
  constructor(
    private boardRepository: HttpBoardRepository
  ) {}

  async createBoard(boardData: { title: string; description?: string; color?: string; isPublic?: boolean }, ownerId: string): Promise<Board> {
    return await this.boardRepository.create({
      title: boardData.title,
      description: boardData.description || '',
      color: boardData.color || '#0079bf',
      ownerId: ownerId,
      memberIds: [ownerId],
      isPublic: boardData.isPublic || false,
      isArchived: false,
      columns: ['Pendiente', 'En Proceso', 'Completado']
    });
  }

  async getUserBoards(userId: string): Promise<Board[]> {
    return await this.boardRepository.findByOwnerId(userId);
  }

  async getBoardDetails(boardId: string): Promise<Board | null> {
    return await this.boardRepository.findById(boardId);
  }
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(
    private userRepository: HttpUserRepository
  ) {}

  async getUserById(id: string): Promise<User | null> {
    return await this.userRepository.findById(id);
  }

  async getUserByEmail(email: string): Promise<User | null> {
    return await this.userRepository.findByEmail(email);
  }

  async getUserByUsername(username: string): Promise<User | null> {
    return await this.userRepository.findByUsername(username);
  }

  async updateUser(id: string, updates: Partial<User>): Promise<User> {
    return await this.userRepository.update(id, updates);
  }
}

@Injectable({
  providedIn: 'root'
})
export class HttpCardRepository implements ICardRepository {
  private apiUrl = `${environment.apiUrl}/cards`;

  constructor(private http: HttpClient) {}

  findById(id: string): Promise<Card | null> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(card => card ? {
        id: card.id || '',
        title: card.title || '',
        description: card.description,
        listId: card.listId || '',
        position: card.position || 0,
        dueDate: card.dueDate ? new Date(card.dueDate) : undefined,
        labels: card.labels || [],
        assigneeId: card.assigneeId,
        attachments: card.attachments || [],
        comments: card.comments || [],
        createdAt: new Date(card.createdAt),
        updatedAt: new Date(card.updatedAt)
      } : null),
      catchError(() => of(null))
    ).toPromise() as any;
  }

  findByListId(listId: string): Promise<Card[]> {
    return this.http.get<any[]>(`${this.apiUrl}/list/${listId}`).pipe(
      map(cards => cards.map(card => ({
        id: card.id || '',
        title: card.title || '',
        description: card.description,
        listId: card.listId || '',
        position: card.position || 0,
        dueDate: card.dueDate ? new Date(card.dueDate) : undefined,
        labels: card.labels || [],
        assigneeId: card.assigneeId,
        attachments: card.attachments || [],
        comments: card.comments || [],
        createdAt: new Date(card.createdAt),
        updatedAt: new Date(card.updatedAt)
      }))),
      catchError(() => of([]))
    ).toPromise() as any;
  }

  findByBoardId(boardId: string): Promise<Card[]> {
    return this.http.get<any[]>(`${this.apiUrl}/board/${boardId}`).pipe(
      map(cards => cards.map(card => ({
        id: card.id || '',
        title: card.title || '',
        description: card.description,
        listId: card.listId || '',
        position: card.position || 0,
        dueDate: card.dueDate ? new Date(card.dueDate) : undefined,
        labels: card.labels || [],
        assigneeId: card.assigneeId,
        attachments: card.attachments || [],
        comments: card.comments || [],
        createdAt: new Date(card.createdAt),
        updatedAt: new Date(card.updatedAt)
      }))),
      catchError(() => of([]))
    ).toPromise() as any;
  }

  create(card: Omit<Card, 'id' | 'createdAt' | 'updatedAt'>): Promise<Card> {
    return this.http.post<any>(this.apiUrl, card).pipe(
      map(card => ({
        id: card.id || '',
        title: card.title || '',
        description: card.description,
        listId: card.listId || '',
        position: card.position || 0,
        dueDate: card.dueDate ? new Date(card.dueDate) : undefined,
        labels: card.labels || [],
        assigneeId: card.assigneeId,
        attachments: card.attachments || [],
        comments: card.comments || [],
        createdAt: new Date(card.createdAt),
        updatedAt: new Date(card.updatedAt)
      }))
    ).toPromise() as any;
  }

  update(id: string, card: Partial<Card>): Promise<Card> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, card).pipe(
      map(card => ({
        id: card.id || '',
        title: card.title || '',
        description: card.description,
        listId: card.listId || '',
        position: card.position || 0,
        dueDate: card.dueDate ? new Date(card.dueDate) : undefined,
        labels: card.labels || [],
        assigneeId: card.assigneeId,
        attachments: card.attachments || [],
        comments: card.comments || [],
        createdAt: new Date(card.createdAt),
        updatedAt: new Date(card.updatedAt)
      }))
    ).toPromise() as any;
  }

  delete(id: string): Promise<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).toPromise() as any;
  }

  updatePositions(listId: string, positions: { id: string; position: number }[]): Promise<void> {
    return this.http.put<void>(`${this.apiUrl}/list/${listId}/positions`, { positions }).toPromise() as any;
  }

  moveToList(cardId: string, newListId: string, position: number): Promise<void> {
    return this.http.put<void>(`${this.apiUrl}/${cardId}/move`, { listId: newListId, position }).toPromise() as any;
  }
}

@Injectable({
  providedIn: 'root'
})
export class CardService {
  constructor(
    private cardRepository: HttpCardRepository
  ) {}

  async getCardsByBoardId(boardId: string): Promise<Card[]> {
    return await this.cardRepository.findByBoardId(boardId);
  }

  async getCardById(id: string): Promise<Card | null> {
    return await this.cardRepository.findById(id);
  }

  async createCard(cardData: { title: string; description?: string; listId: string; position?: number; dueDate?: Date; assigneeId?: string }): Promise<Card> {
    return await this.cardRepository.create({
      title: cardData.title,
      description: cardData.description || '',
      listId: cardData.listId,
      position: cardData.position || 0,
      dueDate: cardData.dueDate,
      labels: [],
      assigneeId: cardData.assigneeId,
      attachments: [],
      comments: []
    });
  }

  async updateCard(id: string, updates: Partial<Card>): Promise<Card> {
    return await this.cardRepository.update(id, updates);
  }

  async deleteCard(id: string): Promise<void> {
    return await this.cardRepository.delete(id);
  }

  async moveCard(cardId: string, newListId: string, position: number): Promise<void> {
    return await this.cardRepository.moveToList(cardId, newListId, position);
  }
}
