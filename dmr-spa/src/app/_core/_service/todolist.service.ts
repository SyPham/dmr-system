import { Injectable } from '@angular/core';
import { PaginatedResult } from '../_model/pagination';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { IToDoList, IToDoListForCancel, IToDoListForReturn } from '../_model/IToDoList';
import { DispatchParams, IDispatch } from '../_model/plan';
import { IMixingInfo } from '../_model/IMixingInfo';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};
@Injectable({
  providedIn: 'root'
})
export class TodolistService {
  baseUrl = environment.apiUrlEC;
  data = new BehaviorSubject<boolean>(null);
  constructor(private http: HttpClient) {}

  setValue(message): void {
    this.data.next(message);
  }
  getValue(): Observable<boolean> {
    return this.data.asObservable();
  }

  cancel(todo: IToDoListForCancel) {
    return this.http.post(`${this.baseUrl}ToDoList/Cancel`, todo);
  }
  cancelRange(todo: IToDoListForCancel[]) {
    return this.http.post(`${this.baseUrl}ToDoList/cancelRange`, todo);
  }
  done(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/Done/' + building, {});
  }
  todo(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/ToDo/' + building, {});
  }
  printGlue(mixingInfoID: number) {
    return this.http.get(this.baseUrl + 'ToDoList/printGlue/' + mixingInfoID, {});
  }
  findPrintGlue(mixingInfoID: number) {
    return this.http.get<IMixingInfo>(this.baseUrl + 'ToDoList/FindPrintGlue/' + mixingInfoID, {});
  }
  dispatch(obj: DispatchParams) {
    return this.http.post<IDispatch[]>(this.baseUrl + 'ToDoList/Dispatch', obj);
  }
  updateStartStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'ToDoList/updateStartStirTimeByMixingInfoID/' + building, {});
  }
  updateFinishStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'ToDoList/updateFinishStirTimeByMixingInfoID/' + building, {});
  }
  generateToDoList(plans: number[]) {
    return this.http.post<IToDoList[]>(this.baseUrl + 'ToDoList/GenerateToDoList', plans);
  }
}
