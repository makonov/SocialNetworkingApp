using SocialNetworkingApp.Models;

namespace SocialNetworkingApp.Interfaces
{
    public interface IGifRepository
    {
        bool Add(Gif gif);
        bool Update(Gif gif);
        bool Delete(Gif gif);
        bool Save();
    }
}
