using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
using SocialNetworkingApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class RedirectControllerTests
    {
        [Fact]
        public void RedirectControllerTests_Index_ShouldRedirectToAdminIndex_WhenUserIsAdmin()
        {
            // Arrange
            var controller = new RedirectController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, UserRoles.Admin)
            };
            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Admin");
        }

        [Fact]
        public void RedirectControllerTests_Index_ShouldRedirectToFeedIndex_WhenUserIsNotAdmin()
        {
            // Arrange
            var controller = new RedirectController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user1")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Feed");
        }

        [Fact]
        public void RedirectControllerTests_Index_ShouldRedirectToFeedIndex_WhenUserHasNoRole()
        {
            // Arrange
            var controller = new RedirectController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user1")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Feed");
        }
    }

}
