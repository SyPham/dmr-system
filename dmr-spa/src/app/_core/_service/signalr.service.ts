import { Injectable } from '@angular/core';
// import * as signalR from '@aspnet/signalr';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  public hubConnection: HubConnection;
  private connectionUrl = environment.scalingHub;
  constructor() {
    // this.connect();
    // this.getConnection().onclose( error => {
    //   setTimeout( () => {
    //     this.startConnection();
    //   }, 3000);
    // });
  }
  public connect = () => {
    this.startConnection();
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
      .then(() => console.log('Scale Machine started signalr!'))
      .catch(err => setTimeout(() => { this.startConnection(); console.log('Scale Machine restarted signalr!'); }, 5000));
  }
}
