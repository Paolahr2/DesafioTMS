// Domain Entities - Representan los conceptos del negocio
// Siguen el principio de Single Responsibility

export interface User {
  id: string;
  email: string;
  username: string;
  fullName: string;
  avatar?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface Board {
  id: string;
  title: string;
  description?: string;
  color?: string;
  ownerId: string;
  memberIds: string[];
  isPublic: boolean;
  isArchived: boolean;
  columns: string[];
  createdAt: Date;
  updatedAt: Date;
}

export interface List {
  id: string;
  title: string;
  boardId: string;
  position: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface Card {
  id: string;
  title: string;
  description?: string;
  listId: string;
  position: number;
  dueDate?: Date;
  labels: Label[];
  assigneeId?: string;
  attachments: Attachment[];
  comments: Comment[];
  createdAt: Date;
  updatedAt: Date;
}

export interface Label {
  id: string;
  name: string;
  color: string;
  boardId: string;
}

export interface Attachment {
  id: string;
  filename: string;
  url: string;
  type: string;
  size: number;
  uploadedBy: string;
  uploadedAt: Date;
}

export interface Comment {
  id: string;
  content: string;
  cardId: string;
  authorId: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  status: number; // 0=Todo, 1=InProgress, 3=Done (TaskStatus enum del backend)
  listId?: string; // Opcional: para asociar con checklists
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  boardId: string;
  assignedToId?: string;
  createdById: string;
  createdByName: string;
  dueDate?: Date;
  completedAt?: Date;
  tags: string[];
  createdAt: Date;
  updatedAt: Date;
  position: number;
  isCompleted: boolean;
  completedBy?: string;
  progressPercentage: number;
  attachments: string[];
}

// Value Objects - Inmutables y sin identidad propia
export class BoardPermissions {
  constructor(
    public readonly canEdit: boolean,
    public readonly canDelete: boolean,
    public readonly canInvite: boolean,
    public readonly canRemoveMembers: boolean
  ) {}

  static owner(): BoardPermissions {
    return new BoardPermissions(true, true, true, true);
  }

  static member(): BoardPermissions {
    return new BoardPermissions(true, false, false, false);
  }

  static viewer(): BoardPermissions {
    return new BoardPermissions(false, false, false, false);
  }
}

// Collaboration Entities - Para gestión de colaboración en tableros
export interface BoardInvitation {
  id: string;
  boardId: string;
  boardTitle: string;
  invitedUserId: string;
  invitedUserEmail: string;
  invitedUserName: string;
  invitedById: string;
  invitedByName: string;
  role: string;
  status: string;
  message?: string;
  createdAt: Date;
  expiresAt?: Date;
}

export interface BoardMember {
  userId: string;
  boardId: string;
  role: 'Owner' | 'Member' | 'Viewer';
  joinedAt: Date;
  userName?: string;
  userEmail?: string;
  userAvatar?: string;
  fullName?: string;
}

export interface InviteUserToBoardRequest {
  Email?: string;
  Username?: string;
  Role?: 'Observer' | 'Editor' | 'Member';
  Message?: string;
  ExpiresAt?: string;
}

export interface RespondToInvitationRequest {
  accept: boolean;
}

// Notification Entity - Para notificaciones persistentes del usuario
export interface Notification {
  id: string;
  userId: string;
  type: 'TaskAssigned' | 'TaskCompleted' | 'TaskDueDate' | 'BoardInvitation' | 'SystemMessage' | 'InvitationAccepted' | 'InvitationRejected';
  title: string;
  message: string;
  isRead: boolean;
  taskId?: string;
  data?: { [key: string]: any };
  createdAt: Date;
  readAt?: Date;
}



