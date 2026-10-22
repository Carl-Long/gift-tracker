using GiftTracker.Domain.Common;
using GiftTracker.Domain.Enums;

namespace GiftTracker.Domain.Entities;

public class Recipient : AuditableEntity
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public RecipientRelationship? Relationship { get; private set; }
    public bool IsArchived { get; private set; }

    private Recipient() { } // For EF core

    public Recipient(
        Guid userId,
        string name,
        string? description,
        DateOnly? birthDate,
        RecipientRelationship? relationship)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A valid user ID is required.", nameof(userId));
        }

        UserId = userId;

        UpdateDetails(name, description, birthDate, relationship);
    }

    public void UpdateDetails(
        string name,
        string? description,
        DateOnly? birthDate,
        RecipientRelationship? relationship)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Recipient name is required.", nameof(name));
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Recipient name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedDescription?.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        if (relationship.HasValue && !Enum.IsDefined(relationship.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(relationship));
        }

        Name = trimmedName;
        Description = trimmedDescription;
        BirthDate = birthDate;
        Relationship = relationship;
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
