using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using UserAPISample2.Controllers;
using UserAPISample2.Models;
using UserAPISample2.Services;

namespace UserAPISample2.Test
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly Mock<IUserService> _userServiceMock;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult()
        {
            // Arrange
            var users = new List<User>
            {
                new() { Id = "1", Name = "John Doe", Username = "johndoe", Email = "johndoe@example.com" },
                new() { Id = "2", Name = "Jane Smith", Username = "janesmith", Email = "janesmith@example.com" }
            };
            _userServiceMock.Setup(service => service.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString()!;
            var user = new User { Id = userId, Name = "John Doe", Username = "johndoe", Email = "johndoe@example.com" };
            _userServiceMock.Setup(service => service.GetUserAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsBadRequestResult()
        {
            // Arrange
            const string invalidId = "invalid";
            _userServiceMock.Setup(service => service.GetUserAsync(invalidId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.GetUserById(invalidId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserById_WithNonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingId = ObjectId.GenerateNewId().ToString()!;
            _userServiceMock.Setup(service => service.GetUserAsync(nonExistingId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.GetUserById(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_WithValidUser_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newUser = new User { Name = "John Doe", Username = "johndoe", Email = "johndoe@example.com" };

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task CreateUser_WithInvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var newUser = new User { Name = "John Doe", Username = "johndoe", Email = "johndoe@example.com" };
            _controller.ModelState.AddModelError("Email", "The Email field is required.");

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateUser_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var invalidUser = new User();
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.CreateUser(invalidUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var invalidUser = new User();
            _controller.ModelState.AddModelError("Email", "The Email field is required.");

            // Act
            var result = await _controller.UpdateUser("1", invalidUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var existingId = ObjectId.GenerateNewId().ToString()!;
            var updatedUser = new User { Name = "Updated Name", Username = "updatedusername", Email = "updatedemail@example..com" };
            _controller.ModelState.AddModelError("Email", "The Email field is not valid.");

            // Act
            var result = await _controller.UpdateUser(existingId, updatedUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithValidIdAndUser_ReturnsNoContentResult()
        {
            // Arrange
            var existingId = ObjectId.GenerateNewId().ToString()!;
            var updatedUser = new User { Name = "Updated Name", Username = "updatedusername", Email = "updatedemail@example.com" };
            _userServiceMock.Setup(service => service.UpdateUserAsync(existingId, updatedUser)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(existingId, updatedUser);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidId_ReturnsBadRequestResult()
        {
            // Arrange
            const string invalidId = "invalid";
            var updatedUser = new User { Name = "Updated Name", Username = "updatedusername", Email = "updatedemail@example.com" };

            // Act
            var result = await _controller.UpdateUser(invalidId, updatedUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithNonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingId = ObjectId.GenerateNewId().ToString()!;
            var updatedUser = new User { Name = "Updated Name", Username = "updatedusername", Email = "updatedemail@example.com" };
            _userServiceMock.Setup(service => service.UpdateUserAsync(nonExistingId, updatedUser)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUser(nonExistingId, updatedUser);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            var existingId = ObjectId.GenerateNewId().ToString()!;
            _userServiceMock.Setup(service => service.DeleteUserAsync(existingId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(existingId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ReturnsBadRequestResult()
        {
            // Arrange
            const string invalidId = "invalid";

            // Act
            var result = await _controller.DeleteUser(invalidId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithNonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingId = ObjectId.GenerateNewId().ToString()!;
            _userServiceMock.Setup(service => service.DeleteUserAsync(nonExistingId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
