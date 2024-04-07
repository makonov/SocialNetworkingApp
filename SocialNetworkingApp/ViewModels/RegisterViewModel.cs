using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Выберите дату рождения"), DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [Required(ErrorMessage = "Введите ваш город")]
        public string City { get; set; }
        [Required(ErrorMessage = "Выберите пол")]
        public bool? IsMale { get; set; }
        [Display(Name = "Электронная почта")]
        [Required(ErrorMessage = "Необходимо ввести адрес электронной почты")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Необходимо ввести пароль")]
        public string Password { get; set; }
        [Display(Name = "Подтвердите пароль")]
        [Required(ErrorMessage = "Необходимо подтвердить пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Введенный пароль не совпадает")]
        public string ConfirmPassword { get; set;}
    }
}
