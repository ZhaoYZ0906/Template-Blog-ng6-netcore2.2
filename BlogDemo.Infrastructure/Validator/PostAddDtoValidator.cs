using BlogDemo.Infrastructure.Dto;
using FluentValidation;

namespace BlogDemo.Infrastructure.Validator
{
    public class PostAddDtoValidator<T> : AbstractValidator<T> where T:FPostDto
    {
        public PostAddDtoValidator()
        {
            RuleFor(x => x.Title).
                NotNull().               
                WithMessage("required|{PropertyName}是必须存在的").
                WithName("标题").
                MaximumLength(50).
                WithMessage("maxlength|{PropertyName}的最大长度是{MaxLength}");

            RuleFor(x => x.Body).
                NotNull().
                WithMessage("required|{PropertyName}不能为空").
                WithName("内容").
                MinimumLength(20).
                WithMessage("minlength|{PropertyName}最小长度是{MinLength}");
        }
    }
}
