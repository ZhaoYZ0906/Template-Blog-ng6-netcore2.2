using BlogDemo.Infrastructure.Dto;
using FluentValidation;

namespace BlogDemo.Infrastructure.Validator
{
    public class PostImageDtoValidator: AbstractValidator<PostImageDto>
    {
        public PostImageDtoValidator()
        {
            RuleFor(x => x.FileName)
                .NotNull()
                .WithName("文件名")
                .WithMessage("required|{PropertyName}是必填项")
                .MaximumLength(100)
                .WithMessage("maxlength|{PropertyName}不可超过{MaxLength}");
        }
    }
}
