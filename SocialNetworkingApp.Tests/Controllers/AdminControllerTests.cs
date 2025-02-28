using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkingApp.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkingApp.Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public void AdminController_Index_ShouldReturnView()
        {
            // Arrange
            var controller = new AdminController();

            // Act
            var result = controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}
