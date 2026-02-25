using AutoMapper;
using HRMS.Application.DTOs.Audit;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Wrappers;

namespace HRMS.Application.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task LogAsync(string entityName, string entityId, AuditAction action, string performedBy,
        string? oldValues = null, string? newValues = null, string? ipAddress = null, string? remarks = null)
    {
        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            Remarks = remarks
        };
        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResponse<IEnumerable<AuditLogResponseDto>>> GetLogsPagedAsync(
        AuditLogFilterDto filter, PaginationParams pagination)
    {
        var (items, total) = await _unitOfWork.AuditLogs.GetLogsPagedAsync(
            filter.EntityName, filter.EntityId, filter.PerformedBy,
            filter.FromDate, filter.ToDate, pagination.Page, pagination.PageSize);
        var dtos = _mapper.Map<IEnumerable<AuditLogResponseDto>>(items);
        return PagedResponse<IEnumerable<AuditLogResponseDto>>.CreateResponse(
            dtos, pagination.Page, pagination.PageSize, total, "OK");
    }

    public async Task<IEnumerable<AuditLogResponseDto>> GetEntityLogsAsync(string entityName, string entityId)
    {
        var logs = await _unitOfWork.AuditLogs.GetByEntityAsync(entityName, entityId);
        return _mapper.Map<IEnumerable<AuditLogResponseDto>>(logs);
    }
}
