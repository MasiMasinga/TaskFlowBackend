using FluentValidation;
using TaskFlow.DTOs.Projects;

namespace TaskFlow.Validators;

public class ProjectListRequestValidator : AbstractValidator<ProjectListRequest>
{
    private static readonly HashSet<string> AllowedSortFields =
        new(StringComparer.OrdinalIgnoreCase) { "name", "createdAt" };

    public ProjectListRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Sort)
            .Must(BeAValidSortValue)
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithMessage($"Sort must be one of: {string.Join(", ", AllowedSortFields)}, optionally prefixed with '-'.");
    }

    private static bool BeAValidSortValue(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return true;
        var field = sort.StartsWith('-') ? sort[1..] : sort;
        return AllowedSortFields.Contains(field);
    }
}