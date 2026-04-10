using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.UserManagement;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public string Language { get; private set; } = "en";
    public string Currency { get; private set; } = "INR";
    public string TimeZone { get; private set; } = "Asia/Kolkata";
    public NotificationPreference NotificationPreferences { get; private set; } = NotificationPreference.Email;
    public bool IsProfilePublic { get; private set; } = false;
    public bool ShareDataWithPartners { get; private set; } = false;
    public bool IsTwoFactorEnabled { get; private set; } = false;
    public string? TwoFactorSecret { get; private set; }
    public string? TwoFactorBackupCodes { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsPhoneVerified { get; private set; } = false;
    public string? Bio { get; private set; }
    public string? DisplayName { get; private set; }
    public DateTime? DataExportRequestedAt { get; private set; }
    public DateTime? DataDeletionRequestedAt { get; private set; }

    public Identity.User User { get; private set; } = null!;

    private UserProfile() { }

    public static UserProfile Create(Guid userId)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Language = "en",
            Currency = "INR",
            TimeZone = "Asia/Kolkata",
            NotificationPreferences = NotificationPreference.Email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePreferences(string language, string currency, string timeZone)
    {
        Language = language ?? Language;
        Currency = currency ?? Currency;
        TimeZone = timeZone ?? TimeZone;
        SetUpdatedAt();
    }

    public void UpdateNotificationPreferences(NotificationPreference preferences)
    {
        NotificationPreferences = preferences;
        SetUpdatedAt();
    }

    public void UpdateProfilePicture(string cdnUrl)
    {
        if (string.IsNullOrWhiteSpace(cdnUrl))
            throw new ArgumentException("CDN URL cannot be empty");

        ProfilePictureUrl = cdnUrl;
        SetUpdatedAt();
    }

    public void UpdatePhone(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
        IsPhoneVerified = false;
        SetUpdatedAt();
    }

    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        SetUpdatedAt();
    }

    public void EnableTwoFactor(string secret, string backupCodes)
    {
        IsTwoFactorEnabled = true;
        TwoFactorSecret = secret;
        TwoFactorBackupCodes = backupCodes;
        SetUpdatedAt();
    }

    public void DisableTwoFactor()
    {
        IsTwoFactorEnabled = false;
        TwoFactorSecret = null;
        TwoFactorBackupCodes = null;
        SetUpdatedAt();
    }

    public void UpdateBio(string? bio, string? displayName)
    {
        Bio = bio;
        DisplayName = displayName;
        SetUpdatedAt();
    }

    public void UpdatePrivacySettings(bool isPublic, bool shareData)
    {
        IsProfilePublic = isPublic;
        ShareDataWithPartners = shareData;
        SetUpdatedAt();
    }

    public void RequestDataExport()
    {
        DataExportRequestedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void RequestDataDeletion()
    {
        DataDeletionRequestedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}