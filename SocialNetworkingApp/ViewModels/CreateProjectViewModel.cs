using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreateProjectViewModel
    {
        [Required(ErrorMessage = "Введите название проекта")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Введите описание проекта")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Введите цель проекта")]
        public string Goal { get; set; }
        public int StatusId { get; set; }
        public int TypeId { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Цель должна быть неотрицательным числом")]
        public decimal? FundraisingGoal { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Прогресс должен быть неотрицательным числом")]
        public decimal? FundraisingProgress { get; set; }
        public IEnumerable<SelectListItem>? Statuses { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
    }
}
