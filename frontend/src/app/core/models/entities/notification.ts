export interface Notification {
  id: string;
  type: 'TaskAssigned' | 'TaskCompleted' | 'TaskDue' | 'TaskCommented' | 'InvitationAccepted' | 'InvitationRejected';
  title: string;
  message: string;
  taskId?: string;
  userId: string;
  isRead: boolean;
  createdAt: Date;
  data?: { [key: string]: any };
}