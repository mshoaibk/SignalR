using Microsoft.AspNetCore.SignalR;

namespace SignalR_BackEnd.Hubservices
{
    public class MyHub : Hub
    {
        // private readonly IchatService _IchatService;
        public MyHub(/*IchatService ichat*/)
        {
            //_IchatService = ichat;
        }

        // Check and return user status (online or offline) based on their SignalR ID
        public bool UserStatus(string UserSignalRId)
        {
            return true; // The logic here checks if a connection ID for UserSignalRId is found in the database and determines online or offline status.
        }

        // When a user opens a new page, a new Connection ID is created, and it's saved in the database.
        public async Task OpenNewPage(string currentUserId, string userName, string type, string brwserInfo)
        {
            // brwserInfo: Store browser info with every new connection.
            // type: Application type (e.g., admin, employee, client).
            var UserSignalRId = currentUserId; // This ID should be unique for each user, typically retrieved from the database.

            // Add the new connection to groups and the database's SignalR Connection Table.
            await Groups.AddToGroupAsync(Context.ConnectionId, UserSignalRId);

            await Clients.Caller.SendAsync("Connected", UserSignalRId); // Send the SignalRUserID if needed.
            await Clients.All.SendAsync("EmpStatus", UserStatus(UserSignalRId), UserSignalRId); // Show that the user is online (this is a toggle function).
        }

        // Remove Connection IDs when a page is closed.
        public async Task LeavePage(string currentUserId, string name, string type, string brwserInfo)
        {
            var UserSignalRId = currentUserId;
            // brwserInfo: Store browser info with every new connection.
            // type: Application type (e.g., admin, employee, client).

            // Remove the connection from groups and the database when the page is closed.
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserSignalRId);
            await Clients.All.SendAsync("EmpStatus", UserStatus(UserSignalRId), UserSignalRId); // Sometimes, this closed page is the last one, and the user appears offline to everyone, depending on your logic.
        }

        // When a user logs out, delete all connections associated with that browser (specific to Chrome).
        public async Task LeaveApplication(string currentUserId, string type, string brwserInfo)
        {
            var UserSignalRId = currentUserId;

            // Get all connections from the database and delete them one by one from the group.

            // Loop and remove all one by one.
            // foreach (var COn in ConnectionIDs)
            // {
            //     if (!string.IsNullOrEmpty(COn))
            //     {
            //         await Groups.RemoveFromGroupAsync(COn, UserSignalRId);
            //     }
            // }

            // Remove connections from the database.
            // await _IchatService.RemoveAllConnectionOfThatUserID(UserSignalRId, ((ChatType)Convert.ToInt64(type)).ToString(), brwserInfo);

            await Clients.All.SendAsync("EmpOrCompStatus", UserStatus(UserSignalRId), UserSignalRId);
        }

        // Send a private message.
        public async Task SendPrivateMessage(string currentUserId, string recipientUserId, string Type, string ReceptType, string message, string filePath, string fileType)
        {
            var checStatus = _IchatService.GetUserOnlineStatus(recipientUserId, ((ChatType)Convert.ToInt64(ReceptType)).ToString()).Result; // Get the recipient's status (offline or online).

            var PrivatechatId = _IchatService.GetChatId(CurrentUserSignalRId, recipientSignalRId, ((ChatType)Convert.ToInt64(Type)).ToString()).Result; // Get the chat ID; if it doesn't exist, create it (this is service logic).

            // Ensure that the sender and recipient are in the same private chat.
            if (!string.IsNullOrEmpty(PrivatechatId))
            {
                _IchatService.AddMessage(currentUserId, recipientUserId, message, PrivatechatId, filePath, fileType).Result; // Save the message in the database.

                if (checStatus == true) // If the recipient is online.
                {
                    // Get all Connection IDs of the recipient.
                    var ConIds = _IchatService.GetAllConnectionOfThatUserID(recipientSignalRId, ((ChatType)Convert.ToInt64(ReceptType)).ToString()).Result;

                    if (ConIds.Count > 0)
                    {
                        foreach (var Con in ConIds)
                        {
                            await Clients.Client(Con).SendAsync("ReceivePrivateMessage", PrivatechatId, message);
                            await Clients.Client(Con).SendAsync("NotifayMe", "You have a new message");
                        }
                    }
                }

                await Clients.Caller.SendAsync("SendMeasseNotifayMe", "Message has been sent", PrivatechatId);
            }
        }

        // Note: This logic is based on my requirements; you can create your own logic based on your requirements.
    }
}
