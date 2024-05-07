namespace SocialNetworkingApp.Interfaces
{
    public interface IPhotoService
    {
        Task<(bool IsAttachedAndExtensionValid, string FileName)> UploadPhotoAsync(IFormFile file, string targetFolder);
        Task<(bool IsReplacementSuccess, string NewFileName)> ReplacePhotoAsync(IFormFile file, string targetFolder, string existingFilePath);
        bool DeleteFolder(string folderPath);
        bool DeletePhoto(string filePath);
        bool IsFileAttachedAndExtensionAllowed(IFormFile file, string[] allowedExtensions);
    }
}
