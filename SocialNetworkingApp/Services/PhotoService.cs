namespace SocialNetworkingApp.Services
{
    using SocialNetworkingApp.Interfaces;
    using System.IO;
    using System.Threading.Tasks;
    public class PhotoService : IPhotoService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static string[] allowedExtensions = { ".gif" };

        public PhotoService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Проверка, разрешено ли расширение файла
        public bool IsFileAttachedAndExtensionAllowed(IFormFile file, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(fileExtension);
        }

        public async Task<(bool IsAttachedAndExtensionValid, string FileName)> UploadPhotoAsync(IFormFile file, string targetFolder)
        {
            // Проверка на наличие файла и его формат
            if (!IsFileAttachedAndExtensionAllowed(file, allowedExtensions))
            {
                return (false, null);
            }

            // Проверка существования папки и создание её, если она не существует
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, targetFolder);
            // Создание всех вложенных папок в целевом пути, если они не существуют
            Directory.CreateDirectory(folderPath);

            // Обработка загрузки файла
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = Guid.NewGuid().ToString() + fileExtension; // создание уникального имени файла
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, fileName);
        }

        public async Task<(bool IsReplacementSuccess, string NewFileName)> ReplacePhotoAsync(IFormFile file, string targetFolder, string existingFilePath)
        {
            // Проверка на наличие файла и его формат
            if (!IsFileAttachedAndExtensionAllowed(file, allowedExtensions))
            {
                return (false, null);
            }

            // Удаление старого файла
            DeletePhoto(existingFilePath);

            // Обработка загрузки нового файла
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var newFileName = Guid.NewGuid().ToString() + fileExtension;
            var newFilePath = Path.Combine(_webHostEnvironment.WebRootPath, targetFolder, newFileName);

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, newFileName);
        }

        // Удаление файла по указанному пути
        public bool DeletePhoto(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('\\'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            return false;
        }

        public bool DeleteFolder(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath.TrimStart('\\'));
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath);
                    return true;
                }
            }
            return false;
        }
    }
}
