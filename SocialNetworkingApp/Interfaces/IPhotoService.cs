namespace SocialNetworkingApp.Interfaces
{
    public interface IPhotoService
    {
        Task<(bool IsUploadedAndExtensionValid, string FileName)> UploadPhotoAsync(IFormFile file, string targetFolder);
        Task<(bool IsReplacementSuccess, string NewFileName)> ReplacePhotoAsync(IFormFile file, string targetFolder, string existingFilePath);
        Task<bool> DeletePhotoAsync(string filePath);
        bool IsFileUploadedAndExtensionAllowed(IFormFile file, string[] allowedExtensions);
    }
}
