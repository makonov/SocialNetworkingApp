using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Interfaces;
using SocialNetworkingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class AdminReferenceControllerTests
    {
        private readonly AdminReferenceController _controller;
        private readonly IServiceProvider _fakeServiceProvider;
        private readonly UserManager<User> _fakeUserManager;

        public AdminReferenceControllerTests()
        {
            _fakeServiceProvider = A.Fake<IServiceProvider>();
            _fakeUserManager = A.Fake<UserManager<User>>();

            _controller = new AdminReferenceController(_fakeServiceProvider, _fakeUserManager);
        }

        [Fact]
        public async Task AdminReferenceController_Index_ValidEntity_ShouldReturnView()
        {
            // Arrange
            var entityName = "User";
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);

            A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult<IEnumerable<User>>(new List<User>()));

            // Act
            var result = await _controller.Index(entityName);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Index");
        }


        [Fact]
        public async Task AdminReferenceController_Index_InvalidEntity_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "NonExistingEntity";

            // Act
            var result = await _controller.Index(entityName);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void AdminReferenceController_Create_ValidEntity_ShouldReturnView()
        {
            // Arrange
            var entityName = "User";

            // Act
            var result = _controller.Create(entityName);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Create");
        }

        [Fact]
        public void AdminReferenceController_Create_InvalidEntity_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "NonExistingEntity";

            // Act
            var result = _controller.Create(entityName);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Create_Post_ValidModel_ShouldRedirect()
        {
            // Arrange
            var entityName = "User";
            var model = new User { UserName = "TestUser" };
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);

            A.CallTo(() => fakeRepository.AddAsync(A<User>.Ignored)).Returns(Task.CompletedTask);

            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "UserName", "TestUser" }
            });
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Form = formCollection }
                }
            };

            // Act
            var result = await _controller.Create(entityName, model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }


        [Fact]
        public async Task AdminReferenceController_Create_Post_InvalidModel_ShouldReturnView()
        {
            // Arrange
            var entityName = "User";
            var model = new User();
            _controller.ModelState.AddModelError("Error", "Invalid model");

            // Act
            var result = await _controller.Create(entityName, model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Create");
        }

        [Fact]
        public async Task AdminReferenceController_Delete_ValidEntity_ShouldRedirect()
        {
            // Arrange
            var entityName = "User";
            var entityId = 1;
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);

            A.CallTo(() => fakeRepository.DeleteAsync(A<int>.Ignored)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(entityName, entityId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task AdminReferenceController_Delete_InvalidEntity_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "NonExistingEntity";
            var entityId = 1;

            // Act
            var result = await _controller.Delete(entityName, entityId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Create_Post_RepositoryIsNull_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "User";
            var model = new User { UserName = "TestUser" };

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(null);

            // Act
            var result = await _controller.Create(entityName, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Create_Post_EntityTypeIsNull_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "NonExistentEntity"; 
            var model = new User { UserName = "TestUser" };

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(A.Fake<IAdminReferenceRepository<User>>());

            // Act
            var result = await _controller.Create(entityName, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Get_RepositoryIsNull_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "User";
            var id = 1;

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(null);

            // Act
            var result = await _controller.Edit(entityName, id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Get_EntityNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);
            A.CallTo(() => fakeRepository.GetByIdAsync(id)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Edit(entityName, id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Get_ValidEntity_ShouldReturnView()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();
            var user = new User { UserName = "TestUser" };

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);
            A.CallTo(() => fakeRepository.GetByIdAsync(id)).Returns(Task.FromResult(user));

            // Act
            var result = await _controller.Edit(entityName, id);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Edit");
            viewResult.Model.Should().Be(user);
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Post_ModelStateIsInvalid_ShouldReturnView()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var model = new User { UserName = "TestUser" };

            _controller.ModelState.AddModelError("UserName", "Invalid");

            // Act
            var result = await _controller.Edit(entityName, id, model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Post_RepositoryIsNull_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var model = new User { UserName = "TestUser" };

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(null);

            // Act
            var result = await _controller.Edit(entityName, id, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Post_EntityTypeIsNull_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "NonExistentEntity"; 
            var id = 1;
            var model = new User { UserName = "TestUser" };

            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();
            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);

            var controller = new AdminReferenceController(_fakeServiceProvider, _fakeUserManager);

            // Act
            var result = await controller.Edit(entityName, id, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task AdminReferenceController_Edit_Post_EntityNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var model = new User { UserName = "TestUser" };
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>)))
                .Returns(fakeRepository);
            A.CallTo(() => fakeRepository.GetByIdAsync(id)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.Edit(entityName, id, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AdminReferenceController_Edit_Post_ValidModel_ShouldRedirectToIndex()
        {
            // Arrange
            var entityName = "User";
            var id = 1;
            var model = new User { UserName = "TestUser" };
            var fakeRepository = A.Fake<IAdminReferenceRepository<User>>();
            var user = new User { UserName = "ExistingUser" };

            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IAdminReferenceRepository<User>))).Returns(fakeRepository);
            A.CallTo(() => fakeRepository.GetByIdAsync(id)).Returns(Task.FromResult(user));
            A.CallTo(() => fakeRepository.UpdateAsync(A<User>.Ignored)).Returns(Task.CompletedTask);

            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "UserName", "TestUser" }
            });

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Form = formCollection }
                }
            };

            // Act
            var result = await _controller.Edit(entityName, id, model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = (RedirectToActionResult)result;
            redirectResult.ActionName.Should().Be("Index");
        }



    }
}
