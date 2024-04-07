namespace SocialNetworkingApp.Services
{
    public class PhotoService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static string[] allowedExtensions;

        public PhotoService(IWebHostEnvironment webHostEnvironment)
        {
            // Инициализация сервиса фотографий с переданным окружением и разрешенными расширениями файлов
            _webHostEnvironment = webHostEnvironment;
            allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        }
    }
}
