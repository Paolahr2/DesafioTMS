// Domain Services - Contienen lógica de negocio pura
// Siguen el principio de Single Responsibility

import { Injectable } from '@angular/core';
import { Board, Card, List, User, BoardPermissions } from '../entities';

@Injectable({
  providedIn: 'root'
})
export class BoardDomainService {
  // Lógica de negocio para tableros

  canUserAccessBoard(userId: string, board: Board): boolean {
    return board.ownerId === userId ||
           board.memberIds.includes(userId) ||
           board.isPublic;
  }

  getUserPermissions(userId: string, board: Board): BoardPermissions {
    if (board.ownerId === userId) {
      return BoardPermissions.owner();
    }

    if (board.memberIds.includes(userId)) {
      return BoardPermissions.member();
    }

    return BoardPermissions.viewer();
  }

  validateBoardCreation(board: Partial<Board>): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!board.title?.trim()) {
      errors.push('El título del tablero es requerido');
    }

    if (!board.ownerId) {
      errors.push('El propietario del tablero es requerido');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }
}

@Injectable({
  providedIn: 'root'
})
export class CardDomainService {
  // Lógica de negocio para tarjetas

  canUserEditCard(userId: string, card: Card, board: Board): boolean {
    const permissions = new BoardDomainService().getUserPermissions(userId, board);
    return permissions.canEdit;
  }

  validateCardCreation(card: Partial<Card>): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!card.title?.trim()) {
      errors.push('El título de la tarjeta es requerido');
    }

    if (!card.listId) {
      errors.push('La lista de la tarjeta es requerida');
    }

    if (card.dueDate && card.dueDate < new Date()) {
      errors.push('La fecha de vencimiento no puede ser en el pasado');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  calculateCardPosition(cards: Card[], newPosition?: number): number {
    if (newPosition !== undefined) {
      return newPosition;
    }

    const maxPosition = cards.length > 0
      ? Math.max(...cards.map(c => c.position))
      : 0;

    return maxPosition + 1;
  }
}

@Injectable({
  providedIn: 'root'
})
export class ListDomainService {
  // Lógica de negocio para listas

  validateListCreation(list: Partial<List>): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!list.title?.trim()) {
      errors.push('El título de la lista es requerido');
    }

    if (!list.boardId) {
      errors.push('El tablero de la lista es requerido');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  calculateListPosition(lists: List[], newPosition?: number): number {
    if (newPosition !== undefined) {
      return newPosition;
    }

    const maxPosition = lists.length > 0
      ? Math.max(...lists.map(l => l.position))
      : 0;

    return maxPosition + 1;
  }
}


