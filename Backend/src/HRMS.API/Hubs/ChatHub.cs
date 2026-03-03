using System.Security.Claims;
using HRMS.Application.DTOs.Chat;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HRMS.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private static readonly Dictionary<string, string> _connections = new();

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        if (employeeId != null)
        {
            _connections[employeeId] = Context.ConnectionId;

            var conversations = await _chatService.GetUserConversationsAsync(Guid.Parse(employeeId));
            foreach (var conv in conversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
            }

            await Clients.Others.SendAsync("UserOnline", employeeId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        if (employeeId != null)
        {
            _connections.Remove(employeeId);
            await Clients.Others.SendAsync("UserOffline", employeeId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(MessageSendDto dto)
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        if (employeeId == null) return;

        var message = await _chatService.SendMessageAsync(Guid.Parse(employeeId), dto);
        await Clients.Group(dto.ConversationId.ToString()).SendAsync("ReceiveMessage", message);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task TypingStarted(string conversationId)
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        var name = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Someone";
        await Clients.OthersInGroup(conversationId).SendAsync("UserTyping", new { employeeId, name, conversationId });
    }

    public async Task TypingStopped(string conversationId)
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        await Clients.OthersInGroup(conversationId).SendAsync("UserStoppedTyping", new { employeeId, conversationId });
    }

    public async Task MarkAsRead(string conversationId)
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        if (employeeId == null) return;

        await _chatService.MarkAsReadAsync(Guid.Parse(conversationId), Guid.Parse(employeeId));
        await Clients.OthersInGroup(conversationId).SendAsync("MessagesRead", new { employeeId, conversationId });
    }
}
