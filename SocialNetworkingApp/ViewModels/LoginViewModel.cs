using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Авторизация")]
        [Required(ErrorMessage = "Введите логин")]
        public string Login {  get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
