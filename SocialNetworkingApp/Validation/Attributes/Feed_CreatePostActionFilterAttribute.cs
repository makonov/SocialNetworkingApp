using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SocialNetworkingApp.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Validation.Attributes
{
    public class Feed_CreatePostActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var viewModel = context.ActionArguments["viewModel"] as CreatePostViewModel;
            if (viewModel != null)
            {
                if (!context.ModelState.IsValid)
                {
                    context.Result = new BadRequestResult();
                }
                else if (viewModel.Text == null && viewModel.Gif == null && viewModel.GifPath == null)
                {

                }
                context.Result = new OkResult();
            }
        }
    }
}
