import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { IRole, IUserRole } from '../_model/role';
import { PaginatedResult } from '../_model/pagination';
@Injectable({
  providedIn: 'root'
})
export class RoleService {
  baseUrl = environment.apiUrlEC;
  messageSource = new BehaviorSubject<number>(0);
  currentMessage = this.messageSource.asObservable();
  // method này để change source message
  changeMessage(message) {
    this.messageSource.next(message);
  }
  constructor(private http: HttpClient) { }
  getAll() {
    return this.http
      .get<IRole[]>(`${this.baseUrl}Role/GetAll`);
  }
  mappingUserRole(userRole: IUserRole) {
    return this.http
      .post(`${this.baseUrl}UserRole/MappingUserRole`, userRole);
  }
  mapUserRole(userID: number, roleID: number) {
    return this.http.put(`${this.baseUrl}UserRole/MapUserRole/${userID}/${roleID}`, {} );
  }
  lock(userRole: IUserRole) {
    return this.http.put(`${this.baseUrl}UserRole/Lock`, userRole);
  }
  isLock(userRole: IUserRole) {
    return this.http.put(`${this.baseUrl}UserRole/IsLock`, userRole);
  }
  getRoleByUserID(userid: number) {
    return this.http.get<IUserRole>(`${this.baseUrl}UserRole/GetRoleByUserID/${userid}`);
  }
}
