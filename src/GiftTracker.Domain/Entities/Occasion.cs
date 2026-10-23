using GiftTracker.Domain.Common;
using GiftTracker.Domain.Models;

namespace GiftTracker.Domain.Entities;

public class Occasion : AuditableEntity
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;
    public const decimal MaxBudget = 99_999_999.99m;

    public Guid OccasionTypeId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int? MaxNumberOfRecipients { get; private set; }
    public decimal? Budget { get; private set; }
    public DateOnly OccasionDate { get; private set; }
    public bool IsArchived { get; private set; }

    private Occasion() { } // For EF Core

    public Occasion(Guid userId, OccasionDetails occasionDetails)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Occasion user ID is required.", nameof(userId));
        }

        UserId = userId;
        UpdateDetails(occasionDetails);
    }

    public void UpdateDetails(OccasionDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        if (details.OccasionTypeId == Guid.Empty)
        {
            throw new ArgumentException("Occasion type is required.", nameof(details));
        }

        if (details.OccasionDate == default)
        {
            throw new ArgumentException("Occasion date is required.", nameof(details));
        }

        var validatedName = ValidateName(details.Name);
        var validatedDescription = ValidateDescription(details.Description);
        ValidateMaxNumberOfRecipients(details.MaxNumberOfRecipients);
        ValidateBudget(details.Budget);

        OccasionTypeId = details.OccasionTypeId;
        Name = validatedName;
        Description = validatedDescription;
        MaxNumberOfRecipients = details.MaxNumberOfRecipients;
        Budget = details.Budget;
        OccasionDate = details.OccasionDate;
    }

    public void Archive() => IsArchived = true;

    public void Restore() => IsArchived = false;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Occasion name is required.", nameof(name));
        }

        var trimmedName = name.Trim();

        return trimmedName.Length > MaxNameLength
            ? throw new ArgumentException($"Occasion name cannot exceed {MaxNameLength} characters.", nameof(name))
            : trimmedName;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmedDescription = description.Trim();

        return trimmedDescription.Length > MaxDescriptionLength
            ? throw new ArgumentException(
                $"Occasion description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description))
            : trimmedDescription;
    }

    private static void ValidateMaxNumberOfRecipients(int? maxNumberOfRecipients)
    {
        if (maxNumberOfRecipients is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxNumberOfRecipients),
                "Maximum number of recipients must be greater than zero.");
        }
    }

    private static void ValidateBudget(decimal? budget)
    {
        if (budget is < 0 or > MaxBudget)
        {
            throw new ArgumentOutOfRangeException(
                nameof(budget),
                $"Occasion budget must be between 0 and {MaxBudget}.");
        }

        if (budget.HasValue && decimal.Round(budget.Value, 2) != budget.Value)
        {
            throw new ArgumentException("Occasion budget cannot exceed two decimal places.", nameof(budget));
        }
    }
}
