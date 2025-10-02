// Repository Interfaces - Abstracciones para acceso a datos
// Siguen el principio de Dependency Inversion

import { User, Board, List, Card, Comment } from '../entities';

export interface IUserRepository {
  findById(id: string): Promise<User | null>;
  findByEmail(email: string): Promise<User | null>;
  findByUsername(username: string): Promise<User | null>;
  create(user: Omit<User, 'id' | 'createdAt' | 'updatedAt'>): Promise<User>;
  update(id: string, user: Partial<User>): Promise<User>;
  delete(id: string): Promise<void>;
}

export interface IBoardRepository {
  findById(id: string): Promise<Board | null>;
  findByOwnerId(ownerId: string): Promise<Board[]>;
  findByMemberId(memberId: string): Promise<Board[]>;
  findPublicBoards(): Promise<Board[]>;
  create(board: Omit<Board, 'id' | 'createdAt' | 'updatedAt'>): Promise<Board>;
  update(id: string, board: Partial<Board>): Promise<Board>;
  delete(id: string): Promise<void>;
  addMember(boardId: string, memberId: string): Promise<void>;
  removeMember(boardId: string, memberId: string): Promise<void>;
}

export interface IListRepository {
  findById(id: string): Promise<List | null>;
  findByBoardId(boardId: string): Promise<List[]>;
  create(list: Omit<List, 'id' | 'createdAt' | 'updatedAt'>): Promise<List>;
  update(id: string, list: Partial<List>): Promise<List>;
  delete(id: string): Promise<void>;
  updatePositions(boardId: string, positions: { id: string; position: number }[]): Promise<void>;
}

export interface ICardRepository {
  findById(id: string): Promise<Card | null>;
  findByListId(listId: string): Promise<Card[]>;
  findByBoardId(boardId: string): Promise<Card[]>;
  create(card: Omit<Card, 'id' | 'createdAt' | 'updatedAt'>): Promise<Card>;
  update(id: string, card: Partial<Card>): Promise<Card>;
  delete(id: string): Promise<void>;
  updatePositions(listId: string, positions: { id: string; position: number }[]): Promise<void>;
  moveToList(cardId: string, newListId: string, position: number): Promise<void>;
}

export interface ICommentRepository {
  findById(id: string): Promise<Comment | null>;
  findByCardId(cardId: string): Promise<Comment[]>;
  create(comment: Omit<Comment, 'id' | 'createdAt' | 'updatedAt'>): Promise<Comment>;
  update(id: string, comment: Partial<Comment>): Promise<Comment>;
  delete(id: string): Promise<void>;
}

// Generic Repository Interface para operaciones comunes
export interface IRepository<T> {
  findById(id: string): Promise<T | null>;
  findAll(): Promise<T[]>;
  create(entity: Omit<T, 'id' | 'createdAt' | 'updatedAt'>): Promise<T>;
  update(id: string, entity: Partial<T>): Promise<T>;
  delete(id: string): Promise<void>;
}


