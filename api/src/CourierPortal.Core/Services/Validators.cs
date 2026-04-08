using CourierPortal.Core.DTOs;
using FluentValidation;

namespace CourierPortal.Core.Services;

public class CreateComplianceProfileValidator : AbstractValidator<CreateComplianceProfileDto>
{
    public CreateComplianceProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CourierType).NotEmpty().MaximumLength(100);
    }
}

public class CreateQuizValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PassMark).InclusiveBetween(0, 100);
    }
}

public class CreateQuizQuestionValidator : AbstractValidator<CreateQuizQuestionDto>
{
    public CreateQuizQuestionValidator()
    {
        RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.QuestionType).NotEmpty().Must(t => t is "multi-choice" or "true-false" or "text");
    }
}

public class SubmitQuizAttemptValidator : AbstractValidator<SubmitQuizAttemptDto>
{
    public SubmitQuizAttemptValidator()
    {
        RuleFor(x => x.QuizId).GreaterThan(0);
        RuleFor(x => x.CourierId).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty();
    }
}

public class CreateJobPostingValidator : AbstractValidator<CreateJobPostingDto>
{
    public CreateJobPostingValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "draft" or "active" or "closed");
    }
}

public class CreateRegistrationFieldValidator : AbstractValidator<CreateRegistrationFieldDto>
{
    public CreateRegistrationFieldValidator()
    {
        RuleFor(x => x.FieldName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).NotEmpty().Must(t => t is "text" or "select" or "checkbox" or "date" or "file");
    }
}

public class CreateDocumentTypeValidator : AbstractValidator<CreateDocumentTypeDto>
{
    public CreateDocumentTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class RequestDriverApprovalValidator : AbstractValidator<RequestDriverApprovalDto>
{
    public RequestDriverApprovalValidator()
    {
        RuleFor(x => x.CourierId).GreaterThan(0);
        RuleFor(x => x.TenantId).GreaterThan(0);
    }
}

public class ReviewDriverApprovalValidator : AbstractValidator<ReviewDriverApprovalDto>
{
    public ReviewDriverApprovalValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "approved" or "rejected");
        RuleFor(x => x.ReviewedBy).NotEmpty();
    }
}

public class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.SenderType).NotEmpty().Must(t => t is "admin" or "courier");
        RuleFor(x => x.SenderName).NotEmpty();
        RuleFor(x => x.MessageText).NotEmpty().MaximumLength(5000);
    }
}

public class CreateConversationValidator : AbstractValidator<CreateConversationDto>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.CourierId).GreaterThan(0);
        RuleFor(x => x.SenderName).NotEmpty();
    }
}
