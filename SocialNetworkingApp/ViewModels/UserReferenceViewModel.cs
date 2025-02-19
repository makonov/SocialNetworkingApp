namespace SocialNetworkingApp.ViewModels
{
    public class UserReferenceViewModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string StudentGroup { get; set; }
        public bool IsMale { get; set; }
        public DateTime BirthDate { get; set; }
        public IList<string> Roles { get; set; }
    }
}
