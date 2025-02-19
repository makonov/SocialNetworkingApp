using SocialNetworkingApp.Data;

namespace SocialNetworkingApp.Middleware
{
    public class RoleBasedRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Проверка аутентификации пользователя
            if (context.User.Identity.IsAuthenticated)
            {
                // Проверяем роль пользователя
                if (context.User.IsInRole(UserRoles.Admin) && context.Request.Path == "/")
                {
                    // Перенаправление для админа на специальную страницу, если он пытается зайти на главную
                    context.Response.Redirect("/Admin/Index"); // Путь к специальной странице администратора
                    return;
                }
                else if (context.User.IsInRole(UserRoles.User) && context.Request.Path == "/")
                {
                    // Перенаправление для обычного пользователя на страницу feed
                    context.Response.Redirect("/feed");
                    return;
                }
            }

            // Если условия не выполняются, передаем запрос дальше
            await _next(context);
        }
    }

}
