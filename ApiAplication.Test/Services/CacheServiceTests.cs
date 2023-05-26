using ApiApplication.Services;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ApiApplication.Tests.Services
{
    public class CacheServiceTests
    {
        private Mock<IDatabase> _databaseMock;
        private CacheService _cacheService;

        public CacheServiceTests()
        {
            _databaseMock = new Mock<IDatabase>();
            _cacheService = new CacheService(_databaseMock.Object);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
        {
            // Arrange
            string key = "myKey";
            _databaseMock.Setup(db => db.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingKey_ShouldReturnFalse()
        {
            // Arrange
            string key = "myKey";
            _databaseMock.Setup(db => db.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
                .ReturnsAsync(false);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAsync_WithExistingKey_ShouldReturnDeserializedValue()
        {
            // Arrange
            string key = "myKey";
            var expectedValue = new TestData { Name = "John Doe" };
            var serializedValue = JsonSerializer.Serialize(expectedValue);
            _databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            // Act
            var result = await _cacheService.GetAsync<TestData>(key);

            // Assert
            Assert.Equal(expectedValue.Name, result.Name);
        }

        [Fact]
        public async Task SetAsync_ShouldSerializeAndStoreValue()
        {
            // Arrange
            string key = "myKey";
            var value = new TestData { Name = "John Doe" };

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            var serialized = JsonSerializer.Serialize(value);
            _databaseMock.Verify(db => db.SetAddAsync(key, serialized, It.IsAny<CommandFlags>()), Times.Once);
        }

        private class TestData
        {
            public string Name { get; set; }
        }
    }
}
