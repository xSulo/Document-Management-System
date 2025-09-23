using FluentValidation;
using dms.Bl.Entities;

namespace dms.Validation;

public class DocumentValidator : AbstractValidator<BlDocument>
{
    static bool ExtOk(string p) =>
        p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
     || p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
     || p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);

    public DocumentValidator()
    {
        RuleSet("Create", () => {
            RuleFor(x => x.Id).Equal(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.FilePath).NotEmpty().Must(ExtOk).WithMessage("Unsupported file type");
        });

        RuleSet("Update", () => {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.FilePath).NotEmpty().Must(ExtOk).WithMessage("Unsupported file type");
        });
    }
}
