using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QRCoder;
using SheetFlow.Hubs;
using System.Collections.Concurrent;

namespace SheetFlow.Controllers;

public class SignatureController : Controller
{
    private static readonly ConcurrentDictionary<string, string> _activeSessions = new();
    private readonly IHubContext<SignatureHub> _hubContext;

    public SignatureController(IHubContext<SignatureHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost]
    public IActionResult CreateSession()
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString("N");
            _activeSessions[sessionId] = string.Empty;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/Signature/Sign?sessionId={sessionId}";
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrCodeData);
            var qrCodeBase64 = qrCode.GetGraphic(20);
            
            var result = new { sessionId, qrCode = $"data:image/png;base64,{qrCodeBase64}" };
            Console.WriteLine($"Session created: {sessionId}, QR Code length: {result.qrCode.Length}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateSession error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult EndSession(string sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            _activeSessions.TryRemove(sessionId, out _);
        }
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Sign(string sessionId, string? fieldId)
    {
        if (!_activeSessions.ContainsKey(sessionId))
            return NotFound("無效的簽名會話");
        
        ViewBag.SessionId = sessionId;
        ViewBag.FieldId = fieldId;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> SaveSignature(string sessionId, string? fieldId, [FromBody] string imageData)
    {
        if (!_activeSessions.ContainsKey(sessionId))
            return NotFound();
        
        _activeSessions[sessionId] = imageData;
        await _hubContext.Clients.Group(sessionId).SendAsync("SignatureUpdated", imageData);
        return Ok();
    }
}
