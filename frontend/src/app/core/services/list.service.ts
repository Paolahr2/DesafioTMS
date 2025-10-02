import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ListItem {
  id?: string;
  text: string;
  completed: boolean;
  notes?: string;
}

export interface ListDto {
  id: string;
  title: string;
  boardId: string;
  order: number;
  items?: ListItem[];
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateListDto {
  title: string;
  boardId: string;
  order: number;
  items?: ListItem[];
  notes?: string;
}

export interface UpdateListDto {
  title: string;
  order: number;
  items?: ListItem[];
}

@Injectable({
  providedIn: 'root'
})
export class ListService {
  private apiUrl = `${environment.apiUrl}/lists`;

  constructor(private http: HttpClient) { }

  getListsByBoardId(boardId: string): Observable<ListDto[]> {
    return this.http.get<ListDto[]>(`${this.apiUrl}/board/${boardId}`);
  }

  createList(listDto: CreateListDto): Observable<ListDto> {
    return this.http.post<ListDto>(this.apiUrl, listDto);
  }

  updateList(id: string, listDto: UpdateListDto): Observable<ListDto> {
    return this.http.put<ListDto>(`${this.apiUrl}/${id}`, listDto);
  }

  deleteList(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}