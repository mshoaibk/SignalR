import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  // first install Signal R package
  //you can write this code in ngONInit
  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:44398/chatHub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      }).build();

      //this function will hit from backend 
    this.hubConnection.on('EmpOrCompStatus', (status: boolean, UserSignalRId: string) => {
    });

    //this will hit when someone send you text message with your connection ID
    this.hubConnection.on('ReceivePrivateMessage', (chatID: string, messageID: number, message: string, chatId: number, senderID: string, date: Date) => {
    });
    this.startConnection();
  }
  startConnection() {
    if (this.hubConnection.state === 'Disconnected') {
      this.hubConnection
        .start()
        .then(() => {
          console.log('SignalR connection started successfully.');
          console.log("ConnectionId :" + this.connectionId);
          // Implement any logic you need after a successful connection
        })
        .catch((error) => {
          console.error('Error starting SignalR connection:', error);
          // You may choose to handle connection startup errors here
        });
    } else {
      console.warn('SignalR connection is already in a connected or connecting state.');

    }
  }

  // call this when open new page 
  openNewPage(currentUserId: string, userName: string): void {
    const brwserInfo = navigator.userAgent;
    // console.log('User-Agent:', userAgent);
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('OpenNewPage', this.companyId.toString(), userName, "1", brwserInfo.toString()).catch((error) => {
        console.error('Error JoinPrivateChat:', error);
      });
    } else {
      console.error('SignalR connection is not in the "Connected" state.');
    }
  }
  // call this when open is closed 
  leavePage(currentUserId: string, name: string): void {
    const brwserInfo = navigator.userAgent;
    // console.log('User-Agent:', userAgent);
    this.hubConnection.invoke('LeavePage', this.companyId.toString(), name, "1", brwserInfo.toString());
  }

  //call this send message
  sendPrivateMessage(recipientUserId: string, message: string, ReceptType: string, image: string): void {
    if (message.trim() == "" || message.trim() == null) {
      return
    }
    // Ensure that the connection is in the 'Connected' state before sending the message
    if (this.hubConnection.state === 'Connected') {
      // Call a server-side hub method to send the private message
      this.hubConnection.invoke('SendPrivateMessage', this.companyId.toString(), recipientUserId, "1", ReceptType, message, image)
        .catch((error) => {
          console.error('Error sending private message:', error);
        });
    } else {
      console.error('SignalR connection is not in the "Connected" state.');
    }

  }

  //call when logout
  logOut() {
    const brwserInfo = navigator.userAgent;
    // console.log('User-Agent:', userAgent);
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('LeaveApplication', this.companyId.toString(), "1", brwserInfo.toString()).catch((error) => {
        console.error('Error JoinPrivateChat:', error);
      });
    } else {
      console.error('SignalR connection is not in the "Connected" state.');
    }
  }

}
