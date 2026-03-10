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
    private readonly ICloudinaryService _cloudinary;

    public ChatController(IChatService service, ICloudinaryService cloudinary)
    {
        _service = service;
        _cloudinary = cloudinary;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty.");
        if (file.Length > 5 * 1024 * 1024) return BadRequest("File size exceeds 5MB limit.");

        var (publicId, url) = await _cloudinary.UploadAsync(file.OpenReadStream(), file.FileName, "chat_attachments");
        return Ok(ApiResponse<object>.Ok(new { url, fileName = file.FileName }, "File uploaded successfully."));
    }

    private Guid? GetEmployeeIdOptional()
    {
        var claim = User.FindFirst("EmployeeId");
        if (claim == null || !Guid.TryParse(claim.Value, out var empId))
            return null;
        return empId;
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] ConversationCreateDto dto)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return BadRequest(ApiResponse<object>.Fail("Only users with an employee profile can create conversations."));
        return Ok(ApiResponse<ConversationResponseDto>.Ok(await _service.CreateConversationAsync(empId.Value, dto)));
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations()
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Ok(ApiResponse<IEnumerable<ConversationResponseDto>>.Ok(Enumerable.Empty<ConversationResponseDto>()));
        
        return Ok(ApiResponse<IEnumerable<ConversationResponseDto>>.Ok(
            await _service.GetUserConversationsAsync(empId.Value)));
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Forbid();
        return Ok(ApiResponse<ConversationResponseDto>.Ok(await _service.GetConversationAsync(id, empId.Value)));
    }

    [HttpGet("conversations/dm/{otherEmployeeId:guid}")]
    public async Task<IActionResult> GetOrCreateDm(Guid otherEmployeeId)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return BadRequest(ApiResponse<object>.Fail("Only users with an employee profile can participate in chats."));
        return Ok(ApiResponse<ConversationResponseDto>.Ok(
            await _service.GetOrCreateDirectConversationAsync(empId.Value, otherEmployeeId)));
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] MessageSendDto dto)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return BadRequest(ApiResponse<object>.Fail("Only users with an employee profile can send messages."));
        return Ok(ApiResponse<MessageResponseDto>.Ok(await _service.SendMessageAsync(empId.Value, dto)));
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] PaginationParams pagination)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Forbid();
        return Ok(await _service.GetMessagesPagedAsync(conversationId, empId.Value, pagination));
    }

    [HttpPut("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Forbid();
        await _service.MarkAsReadAsync(conversationId, empId.Value);
        return Ok(ApiResponse<object>.Ok(null!, "Messages marked as read."));
    }

    [HttpPut("messages/{messageId:guid}/retract")]
    public async Task<IActionResult> RetractMessage(Guid messageId)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Forbid();
        return Ok(ApiResponse<MessageResponseDto>.Ok(await _service.RetractMessageAsync(messageId, empId.Value)));
    }

    [HttpPost("conversations/{conversationId:guid}/members/{employeeId:guid}")]
    public async Task<IActionResult> AddMember(Guid conversationId, Guid employeeId)
    {
        var empId = GetEmployeeIdOptional();
        if (empId == null) return Forbid();
        await _service.AddMemberAsync(conversationId, employeeId, empId.Value);
        return Ok(ApiResponse<object>.Ok(null!, "Member added."));
    }
}
