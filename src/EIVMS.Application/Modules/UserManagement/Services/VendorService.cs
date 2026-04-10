using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Organization;
using EIVMS.Application.Modules.UserManagement.DTOs.Vendor;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Domain.Entities.UserManagement;
using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.Services;

public class VendorService : IVendorService
{
    private readonly IUserManagementRepository _repository;
    private readonly IOrganizationService _organizationService;

    public VendorService(IUserManagementRepository repository, IOrganizationService organizationService)
    {
        _repository = repository;
        _organizationService = organizationService;
    }

    public async Task<ApiResponse<VendorApplicationResponseDto>> ApplyForVendorAsync(CreateVendorApplicationDto dto, Guid userId)
    {
        var existingApp = await _repository.GetVendorApplicationByUserIdAsync(userId);
        if (existingApp != null && existingApp.Status != VendorOnboardingStatus.Rejected)
            return ApiResponse<VendorApplicationResponseDto>.ErrorResponse("You already have an active vendor application", 409);

        var orgResult = await _organizationService.CreateOrganizationAsync(new CreateOrganizationDto
        {
            Name = dto.BusinessName,
            Description = dto.BusinessDescription,
            Type = OrganizationType.Vendor
        }, userId);

        if (!orgResult.Success || orgResult.Data == null)
            return ApiResponse<VendorApplicationResponseDto>.ErrorResponse("Failed to create organization");

        var application = VendorApplication.Create(orgResult.Data.Id, userId, dto.BusinessName, dto.BusinessType, dto.BusinessDescription);
        application.UpdateBusinessDetails(dto.BusinessName, dto.BusinessType, dto.BusinessDescription, dto.GstNumber, dto.BusinessLicenseNumber);

        var created = await _repository.CreateVendorApplicationAsync(application);

        await _repository.AddVendorAuditLogAsync(VendorApplicationAuditLog.Create(created.Id, VendorOnboardingStatus.Draft, VendorOnboardingStatus.Draft, "APPLICATION_CREATED", userId));

        return ApiResponse<VendorApplicationResponseDto>.SuccessResponse(MapToDto(created), "Vendor application created successfully", 201);
    }

    public async Task<ApiResponse<VendorApplicationResponseDto>> GetMyApplicationAsync(Guid userId)
    {
        var application = await _repository.GetVendorApplicationByUserIdAsync(userId);
        if (application == null)
            return ApiResponse<VendorApplicationResponseDto>.ErrorResponse("No vendor application found", 404);

        return ApiResponse<VendorApplicationResponseDto>.SuccessResponse(MapToDto(application));
    }

    public async Task<ApiResponse<bool>> UpdateBankDetailsAsync(Guid applicationId, UpdateBankDetailsDto dto, Guid userId)
    {
        var application = await _repository.GetVendorApplicationByIdAsync(applicationId);
        if (application == null)
            return ApiResponse<bool>.ErrorResponse("Application not found", 404);
        if (application.ApplicantUserId != userId)
            return ApiResponse<bool>.ErrorResponse("Access denied", 403);
        if (application.Status != VendorOnboardingStatus.Draft && application.Status != VendorOnboardingStatus.DocumentsRequired)
            return ApiResponse<bool>.ErrorResponse("Cannot update bank details in current status");

        application.UpdateBankDetails(dto.BankAccountNumber, dto.BankIfscCode, dto.BankName, dto.AccountHolderName);
        await _repository.UpdateVendorApplicationAsync(application);

        return ApiResponse<bool>.SuccessResponse(true, "Bank details updated");
    }

    public async Task<ApiResponse<bool>> SubmitApplicationAsync(Guid applicationId, Guid userId)
    {
        var application = await _repository.GetVendorApplicationByIdAsync(applicationId);
        if (application == null)
            return ApiResponse<bool>.ErrorResponse("Application not found", 404);
        if (application.ApplicantUserId != userId)
            return ApiResponse<bool>.ErrorResponse("Access denied", 403);

        if (string.IsNullOrWhiteSpace(application.BankAccountNumber))
            return ApiResponse<bool>.ErrorResponse("Bank details are required before submission");

        var previousStatus = application.Status;
        application.Submit();
        await _repository.UpdateVendorApplicationAsync(application);

        await _repository.AddVendorAuditLogAsync(VendorApplicationAuditLog.Create(applicationId, previousStatus, VendorOnboardingStatus.Submitted, "APPLICATION_SUBMITTED", userId));

        return ApiResponse<bool>.SuccessResponse(true, "Application submitted successfully. You will be notified within 3-5 business days.");
    }

    public async Task<ApiResponse<List<VendorApplicationResponseDto>>> GetAllApplicationsAsync(string? status = null)
    {
        VendorOnboardingStatus? filterStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VendorOnboardingStatus>(status, true, out var parsedStatus))
            filterStatus = parsedStatus;

        var applications = await _repository.GetAllVendorApplicationsAsync(filterStatus);
        var dtos = applications.Select(MapToDto).ToList();

        return ApiResponse<List<VendorApplicationResponseDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<VendorApplicationResponseDto>> GetApplicationByIdAsync(Guid applicationId)
    {
        var application = await _repository.GetVendorApplicationByIdAsync(applicationId);
        if (application == null)
            return ApiResponse<VendorApplicationResponseDto>.ErrorResponse("Application not found", 404);

        return ApiResponse<VendorApplicationResponseDto>.SuccessResponse(MapToDto(application));
    }

    public async Task<ApiResponse<bool>> ReviewApplicationAsync(Guid applicationId, ReviewVendorApplicationDto dto, Guid adminUserId)
    {
        var application = await _repository.GetVendorApplicationByIdAsync(applicationId);
        if (application == null)
            return ApiResponse<bool>.ErrorResponse("Application not found", 404);

        if (application.Status != VendorOnboardingStatus.Submitted && application.Status != VendorOnboardingStatus.UnderReview)
            return ApiResponse<bool>.ErrorResponse($"Cannot review application in {application.Status} status");

        var previousStatus = application.Status;

        if (application.Status == VendorOnboardingStatus.Submitted)
            application.StartReview(adminUserId);

        if (dto.IsApproved)
        {
            application.Approve(adminUserId, dto.Notes);

            var org = await _repository.GetOrganizationByIdAsync(application.OrganizationId);
            org?.Activate();
            if (org != null)
                await _repository.UpdateOrganizationAsync(org);

            await _repository.AddVendorAuditLogAsync(VendorApplicationAuditLog.Create(applicationId, previousStatus, VendorOnboardingStatus.Approved, "APPLICATION_APPROVED", adminUserId, dto.Notes));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return ApiResponse<bool>.ErrorResponse("Rejection reason is required");

            application.Reject(adminUserId, dto.RejectionReason!);

            await _repository.AddVendorAuditLogAsync(VendorApplicationAuditLog.Create(applicationId, previousStatus, VendorOnboardingStatus.Rejected, "APPLICATION_REJECTED", adminUserId, dto.RejectionReason));
        }

        await _repository.UpdateVendorApplicationAsync(application);

        var message = dto.IsApproved ? "Application approved. Vendor account activated." : "Application rejected.";
        return ApiResponse<bool>.SuccessResponse(true, message);
    }

    private static VendorApplicationResponseDto MapToDto(VendorApplication app)
    {
        return new VendorApplicationResponseDto
        {
            Id = app.Id,
            OrganizationId = app.OrganizationId,
            BusinessName = app.BusinessName,
            BusinessType = app.BusinessType,
            Status = app.Status,
            GstNumber = app.GstNumber,
            ReviewNotes = app.ReviewNotes,
            RejectionReason = app.RejectionReason,
            SubmittedAt = app.SubmittedAt,
            ReviewedAt = app.ReviewedAt,
            ResubmissionCount = app.ResubmissionCount,
            HasGstDocument = !string.IsNullOrWhiteSpace(app.GstDocumentUrl),
            HasBusinessLicense = !string.IsNullOrWhiteSpace(app.BusinessLicenseUrl),
            HasPanCard = !string.IsNullOrWhiteSpace(app.PanCardUrl),
            HasBankStatement = !string.IsNullOrWhiteSpace(app.BankStatementUrl),
            HasBankDetails = !string.IsNullOrWhiteSpace(app.BankAccountNumber),
            CreatedAt = app.CreatedAt,
            AuditLogs = app.AuditLogs.OrderByDescending(a => a.CreatedAt).Select(a => new VendorAuditLogDto
            {
                Action = a.Action,
                FromStatus = a.FromStatus.ToString(),
                ToStatus = a.ToStatus.ToString(),
                Notes = a.Notes,
                PerformedAt = a.CreatedAt
            }).ToList()
        };
    }
}