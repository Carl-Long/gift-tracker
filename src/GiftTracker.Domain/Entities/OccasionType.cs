using GiftTracker.Domain.Common;

namespace GiftTracker.Domain.Entities;

public class OccasionType : AuditableEntity
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsArchived { get; private set; }

    private OccasionType() { } // For EF core

    public OccasionType(string name, string? description)
    {
        UpdateDetails(name, description);
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Occasion name cannot exceed {MaxNameLength}", nameof(name));
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Occasion name cannot exceed {MaxDescriptionLength}", nameof(name));
        }

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedDescription?.Length > MaxDescriptionLength)
        {
            throw new ArgumentException("Occasion description is too long", nameof(description));
        }

        Name = trimmedName;
        Description = trimmedDescription;
    }

    public void Archive()
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
    }

    public void Restore()
    {
        if (!IsArchived)
        {
            return;
        }

        IsArchived = false;
    }
}
