using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SheetFlow.Hubs;

[AllowAnonymous]
public class SignatureHub : Hub
{
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }

    public async Task UpdateSignature(string sessionId, string imageData)
    {
        await Clients.Group(sessionId).SendAsync("SignatureUpdated", imageData);
    }

    public async Task RequestSignature(string sessionId)
    {
        await Clients.Group(sessionId).SendAsync("SignatureRequested");
    }
}
