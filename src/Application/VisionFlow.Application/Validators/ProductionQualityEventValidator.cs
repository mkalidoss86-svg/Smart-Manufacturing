using FluentValidation;

namespace VisionFlow.Application.Validators;

public class ProductionQualityEventValidator : AbstractValidator<DTOs.ProductionQualityEventDto>
{
    public ProductionQualityEventValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required")
            .MaximumLength(100)
            .WithMessage("ProductId cannot exceed 100 characters");

        RuleFor(x => x.BatchId)
            .NotEmpty()
            .WithMessage("BatchId is required")
            .MaximumLength(100)
            .WithMessage("BatchId cannot exceed 100 characters");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => new[] { "Pass", "Fail", "Warning", "Pending" }.Contains(status))
            .WithMessage("Status must be one of: Pass, Fail, Warning, Pending");

        RuleFor(x => x.QualityMetrics)
            .NotEmpty()
            .WithMessage("QualityMetrics cannot be empty")
            .Must(metrics => metrics != null && metrics.Count > 0)
            .WithMessage("At least one quality metric is required");

        When(x => x.StationId != null, () =>
        {
            RuleFor(x => x.StationId)
                .MaximumLength(100)
                .WithMessage("StationId cannot exceed 100 characters");
        });
    }
}
