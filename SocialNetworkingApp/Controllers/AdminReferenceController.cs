using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocialNetworkingApp.Data;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using SocialNetworkingApp.Repositories;

namespace SocialNetworkingApp.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    [Route("admin/reference/{entityName}")]
    public class AdminReferenceController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;

        public AdminReferenceController(IServiceProvider serviceProvider, UserManager<User> userManager)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string entityName)
        {
            var repository = GetRepository(entityName);
            if (repository == null) return NotFound();

            var entities = await repository.GetAllAsync();
            ViewBag.EntityName = entityName;

            return View("Index", entities);
        }

        [HttpGet("create")]
        public IActionResult Create(string entityName)
        {
            var entityType = GetEntityType(entityName);
            if (entityType == null) return NotFound();

            ViewBag.EntityName = entityName;
            return View("Create", Activator.CreateInstance(entityType));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string entityName, [FromForm] object model)
        {
            if (!ModelState.IsValid)
                return View("Create", model);

            var repository = GetRepository(entityName);
            if (repository == null) return NotFound();

            var entityType = GetEntityType(entityName);
            if (entityType == null) return NotFound();

            // Создаем экземпляр модели с нужным типом
            var typedModel = Activator.CreateInstance(entityType);

            // Привязываем значения из формы к свойствам модели
            foreach (var prop in entityType.GetProperties())
            {
                var formValue = Request.Form[prop.Name].ToString();

                if (!string.IsNullOrEmpty(formValue))
                {
                    var convertedValue = Convert.ChangeType(formValue, prop.PropertyType);
                    prop.SetValue(typedModel, convertedValue);
                }
            }

            // Получаем метод AddAsync у репозитория
            var method = repository.GetType().GetMethod("AddAsync");
            if (method != null)
            {
                // Вызываем AddAsync с конкретным типом сущности
                await (Task)method.Invoke(repository, new object[] { typedModel });
            }

            // Перенаправление после успешного добавления
            return RedirectToAction("Index", new { entityName });
        }




        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string entityName, int id)
        {
            var repository = GetRepository(entityName);
            if (repository == null) return NotFound();

            var entity = await repository.GetByIdAsync(id);
            if (entity == null) return NotFound();

            ViewBag.EntityName = entityName;
            ViewBag.EntityId = id;
            return View("Edit", entity);
        }



        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(string entityName, int id, [FromForm] object model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            var repository = GetRepository(entityName);
            if (repository == null) return NotFound();

            var entityType = GetEntityType(entityName);
            if (entityType == null) return NotFound();

            // Получаем объект по ID
            var entity = await repository.GetByIdAsync(id);
            if (entity == null) return NotFound();

            // Привязываем значения из формы к свойствам модели
            foreach (var prop in entityType.GetProperties())
            {
                var formValue = Request.Form[prop.Name].ToString();

                if (!string.IsNullOrEmpty(formValue))
                {
                    var convertedValue = Convert.ChangeType(formValue, prop.PropertyType);
                    prop.SetValue(entity, convertedValue);
                }
            }

            // Получаем метод UpdateAsync у репозитория
            var method = repository.GetType().GetMethod("UpdateAsync");
            if (method != null)
            {
                // Вызываем UpdateAsync с конкретным типом сущности
                await (Task)method.Invoke(repository, new object[] { entity });
            }

            // Перенаправление после успешного обновления
            return RedirectToAction("Index", new { entityName });
        }


        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(string entityName, int id)
        {
            var repository = GetRepository(entityName);
            if (repository == null) return NotFound();

            var entityType = GetEntityType(entityName);
            if (entityType == null) return NotFound();

            // Получаем объект по ID
            var entity = await repository.GetByIdAsync(id);
            if (entity == null) return NotFound();

            // Получаем метод DeleteAsync у репозитория
            var method = repository.GetType().GetMethod("DeleteAsync");
            if (method != null)
            {
                // Вызываем DeleteAsync с конкретным типом сущности
                await (Task)method.Invoke(repository, new object[] { id });
            }

            // Перенаправление после успешного удаления
            return RedirectToAction("Index", new { entityName });
        }


        private dynamic GetRepository(string entityName)
        {
            var entityType = GetEntityType(entityName);
            if (entityType == null) return null;

            var repositoryType = typeof(IAdminReferenceRepository<>).MakeGenericType(entityType);
            return _serviceProvider.GetService(repositoryType);
        }


        private Type GetEntityType(string entityName)
        {
            return entityName switch
            {
                "PostType" => typeof(PostType),
                "User" => typeof(User),
                "CommunityType" => typeof(CommunityType),
                "ProjectStatus" => typeof(ProjectStatus),
                "ProjectType" => typeof(ProjectType),
                "StudentGroup" => typeof(StudentGroup),
                _ => null
            };
        }
    }
}
