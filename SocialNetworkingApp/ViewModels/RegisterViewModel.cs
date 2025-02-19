using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Выберите дату рождения"), DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [Required(ErrorMessage = "Выберите свою группу")]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "Выберите пол")]
        public bool? IsMale { get; set; }
        [Display(Name = "Имя пользователя")]
        [Required(ErrorMessage = "Необходимо ввести логин")]
        public string Login { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Необходимо ввести пароль")]
        public string Password { get; set; }
        [Display(Name = "Подтвердите пароль")]
        [Required(ErrorMessage = "Необходимо подтвердить пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Введенный пароль не совпадает")]
        public string ConfirmPassword { get; set;}

        public List<SelectListItem>? Groups { get; set; }
    }
}
