using System.Security.Claims;
using HRMS.Application.DTOs.Chat;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _service;
    public ChatController(IChatService service) => _service = service;

    private Guid GetEmployeeId()
    {
        var claim = User.FindFirst("EmployeeId");
        if (claim == null || !Guid.TryParse(claim.Value, out var empId))
            throw new HRMS.Shared.Exceptions.UnauthorizedException("Employee ID claim missing or invalid. Please re-login.");
        return empId;
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] ConversationCreateDto dto)
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<ConversationResponseDto>.Ok(await _service.CreateConversationAsync(empId, dto)));
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations()
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<IEnumerable<ConversationResponseDto>>.Ok(
            await _service.GetUserConversationsAsync(empId)));
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<ConversationResponseDto>.Ok(await _service.GetConversationAsync(id, empId)));
    }

    [HttpGet("conversations/dm/{otherEmployeeId:guid}")]
    public async Task<IActionResult> GetOrCreateDm(Guid otherEmployeeId)
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<ConversationResponseDto>.Ok(
            await _service.GetOrCreateDirectConversationAsync(empId, otherEmployeeId)));
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] MessageSendDto dto)
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<MessageResponseDto>.Ok(await _service.SendMessageAsync(empId, dto)));
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] PaginationParams pagination)
    {
        var empId = GetEmployeeId();
        return Ok(await _service.GetMessagesPagedAsync(conversationId, empId, pagination));
    }

    [HttpPut("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var empId = GetEmployeeId();
        await _service.MarkAsReadAsync(conversationId, empId);
        return Ok(ApiResponse<object>.Ok(null!, "Messages marked as read."));
    }

    [HttpPut("messages/{messageId:guid}/retract")]
    public async Task<IActionResult> RetractMessage(Guid messageId)
    {
        var empId = GetEmployeeId();
        return Ok(ApiResponse<MessageResponseDto>.Ok(await _service.RetractMessageAsync(messageId, empId)));
    }

    [HttpPost("conversations/{conversationId:guid}/members/{employeeId:guid}")]
    public async Task<IActionResult> AddMember(Guid conversationId, Guid employeeId)
    {
        var empId = GetEmployeeId();
        await _service.AddMemberAsync(conversationId, employeeId, empId);
        return Ok(ApiResponse<object>.Ok(null!, "Member added."));
    }
}
