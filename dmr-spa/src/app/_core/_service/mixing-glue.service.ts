import { Injectable } from '@angular/core';
// import * as signalR from '@aspnet/signalr';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class MixingGlueService {
  public hubConnection: HubConnection;
  private connectionUrl = environment.scalingHub;
  public info = new BehaviorSubject<{ scalingMachineID: string, message: string, unit: string}>(null);
  constructor() {
  }
  public setValue(message): void {
    this.info.next(message);
  }
  public getValue(): Observable<{ scalingMachineID: string, message: string, unit: string }> {
    return this.info.asObservable();
  }
  public connect = () => {
    this.startConnection();
    this.addListeners();
  }
  private getConnection(): HubConnection {
    return new HubConnectionBuilder()
      .withUrl(this.connectionUrl)
      .build();
  }
  private startConnection = () => {
    this.hubConnection = this.getConnection();
    this.hubConnection
      .start()
      .then(() => console.log('Đã kết nối tới cân!'))
      .catch(err => setTimeout(() => { this.startConnection(); console.warn('Mất kết nối! Khởi động lại!'); }, 5000));
  }
  private addListeners(): void {
    this.hubConnection.on('Welcom', (scalingMachineID, message, unit) => {
      this.setValue({ scalingMachineID, message, unit });
    });
  }
}
