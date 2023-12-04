using MongoDB.Driver;
using Moq;
using UserAPISample2.DAL;
using UserAPISample2.Models;
using UserAPISample2.Services;
using UserAPISample2.Settings;

namespace UserAPISample2.Test
{
    public class UserServiceTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var mockSettings = new Mock<IUsersDatabaseSettings>();
            mockSettings.SetupGet(s => s.UsersCollectionName).Returns("Users");
            mockSettings.SetupGet(s => s.CountersCollectionName).Returns("counters");

            _mockMongoDbContext = new Mock<IMongoDbContext>();

            _userService = new UserService(_mockMongoDbContext.Object, mockSettings.Object);
        }

        [Fact]
        public async Task GetUserAsync_ValidId_ReturnsUser()
        {
            // Arrange
            const string id = "validId";
            var expectedUser = new User { Id = id, Name = "John Doe" };
            var mockUsers = new Mock<IAsyncCursor<User>>();
            mockUsers.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockUsers.SetupSequence(m => m.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            mockUsers.SetupGet(m => m.Current).Returns(new List<User> { expectedUser });

            var mockCollection = new Mock<IMongoCollection<User>>();
            mockCollection.Setup(m => m.FindAsync(
                It.IsAny<FilterDefinition<User>>(), 
                It.IsAny<FindOptions<User>>(), 
                default
                )).ReturnsAsync(mockUsers.Object);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.GetUserAsync(id);

            // Assert
            Assert.Equal(expectedUser, result);
            mockCollection.Verify(m => m.FindAsync(
                It.IsAny<FilterDefinition<User>>(), 
                It.IsAny<FindOptions<User>>(), 
                default
            ), Times.Once);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsListOfUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new() { Id = "1", Name = "John Doe" },
                new() { Id = "2", Name = "Jane Smith" }
            };
            var mockUsers = new Mock<IAsyncCursor<User>>();
            mockUsers.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockUsers.SetupSequence(m => m.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            mockUsers.SetupGet(m => m.Current).Returns(expectedUsers);

            var mockCollection = new Mock<IMongoCollection<User>>();
            mockCollection.Setup(m => m.FindAsync(
                It.IsAny<FilterDefinition<User>>(), 
                It.IsAny<FindOptions<User>>(), 
                default
            )).ReturnsAsync(mockUsers.Object);
            
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.GetUsersAsync();

            // Assert
            Assert.Equal(expectedUsers, result);
            mockCollection.Verify(m => m.FindAsync(
                It.IsAny<FilterDefinition<User>>(), 
                It.IsAny<FindOptions<User>>(), 
                default
            ), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var user = new User { Id = "1", Name = "John Doe" };
            var mockCollection = new Mock<IMongoCollection<User>>();
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.CreateUserAsync(user);

            // Assert
            Assert.Equal(user, result);
            mockCollection.Verify(m => m.InsertOneAsync(user, It.IsAny<InsertOneOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ValidUser_ReturnsTrue()
        {
            // Arrange
            const string id = "validId";
            var user = new User { Id = id, Name = "John Doe" };
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockReplaceResult = new Mock<ReplaceOneResult>();
            mockReplaceResult.Setup(m => m.ModifiedCount).Returns(1);
            mockCollection.Setup(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<User>>(), user, It.IsAny<ReplaceOptions>(), default)).ReturnsAsync(mockReplaceResult.Object);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.UpdateUserAsync(id, user);

            // Assert
            Assert.True(result);
            mockCollection.Verify(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<User>>(), user, It.IsAny<ReplaceOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_InvalidUser_ReturnsFalse()
        {
            // Arrange
            const string id = "invalidId";
            var user = new User { Id = id, Name = "John Doe" };
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockReplaceResult = new Mock<ReplaceOneResult>();
            mockReplaceResult.Setup(m => m.ModifiedCount).Returns(0);
            mockCollection.Setup(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<User>>(), user, It.IsAny<ReplaceOptions>(), default)).ReturnsAsync(mockReplaceResult.Object);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.UpdateUserAsync(id, user);

            // Assert
            Assert.False(result);
            mockCollection.Verify(m => m.ReplaceOneAsync(It.IsAny<FilterDefinition<User>>(), user, It.IsAny<ReplaceOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            const string id = "validId";
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockDeleteResult = new Mock<DeleteResult>();
            mockDeleteResult.Setup(m => m.DeletedCount).Returns(1);
            mockCollection.Setup(m => m.DeleteOneAsync(It.IsAny<FilterDefinition<User>>(), default)).ReturnsAsync(mockDeleteResult.Object);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.DeleteUserAsync(id);

            // Assert
            Assert.True(result);
            mockCollection.Verify(m => m.DeleteOneAsync(It.IsAny<FilterDefinition<User>>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            const string id = "invalidId";
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockDeleteResult = new Mock<DeleteResult>();
            mockDeleteResult.Setup(m => m.DeletedCount).Returns(0);
            mockCollection.Setup(m => m.DeleteOneAsync(It.IsAny<FilterDefinition<User>>(), default)).ReturnsAsync(mockDeleteResult.Object);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<User>(It.IsAny<string>()))
                .Returns(mockCollection.Object);

            // Act
            var result = await _userService.DeleteUserAsync(id);

            // Assert
            Assert.False(result);
            mockCollection.Verify(m => m.DeleteOneAsync(It.IsAny<FilterDefinition<User>>(), default), Times.Once);
        }
    }
}

