using C3D.IISTestapp.Tests;
using System.Text.Json;
using Xunit.Abstractions;

namespace C3D.IISOwinApp.Tests
{
    public class UnitTest1 : IClassFixture<IISExpressFixture>
    {
        private readonly ITestOutputHelper outputHelper;
        private readonly IISExpressFixture fixture;

        public UnitTest1(ITestOutputHelper outputHelper, IISExpressFixture fixture)
        {
            this.outputHelper = outputHelper;
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestResponse5555()
        {
            const int port = 5555;
            string expectedResponse = $"Hello from unit test {port}";

            // Arrange
            using var iis = await fixture.StartAsync(port, outputHelper,
                ("AppOptions__TestResponse", expectedResponse)
                );

            var client = fixture.CreateClient(port);

            // Act
            var response = await client.GetAsync("/api/values");
            var responseString = await response.Content.ReadAsStringAsync();

            outputHelper.WriteLine($"ResponseCode: {response.StatusCode}");
            outputHelper.WriteLine($"Response: {responseString}");

            // Assert
            Assert.True(response.IsSuccessStatusCode, "Response was not successful");
            Assert.Equal(expectedResponse, responseString);
        }

        [Fact]
        public async Task TestCurrentTime5556()
        {
            const int port = 5556;

            // Arrange
            using var iis = await fixture.StartAsync(port, outputHelper);

            var client = fixture.CreateClient(port);

            // Act
            var response = await client.GetAsync("/ts");
            var responseString = await response.Content.ReadAsStringAsync();

            outputHelper.WriteLine($"ResponseCode: {response.StatusCode}");
            outputHelper.WriteLine($"Response: {responseString}");

            // Assert
            Assert.True(response.IsSuccessStatusCode, "Response was not successful");

            Assert.True(DateTimeOffset.TryParse(responseString, out var responseTime), "Response was not a valid DateTimeOffset");
            outputHelper.WriteLine($"ResponseTime: {responseTime}");
            Assert.Equal(DateTimeOffset.UtcNow, responseTime, TimeSpan.FromSeconds(5)); // allow 5 seconds of difference
        }

        [Fact]
        public async Task TestTestTime5557()
        {
            const int port = 5557;
            
            var testTime = DateTimeOffset.UtcNow.AddHours(-10);

            outputHelper.WriteLine($"TestTime: {testTime}");

            var testTimeString = testTime.ToString("O");

            // Arrange
            using var iis = await fixture.StartAsync(port, outputHelper,
                ("AppOptions__TestTime", testTimeString)
                );

            var client = fixture.CreateClient(port);

            // Act
            var response = await client.GetAsync("/ts");
            var responseString = await response.Content.ReadAsStringAsync();

            outputHelper.WriteLine($"ResponseCode: {response.StatusCode}");
            outputHelper.WriteLine($"Response: {responseString}");

            // Assert
            Assert.True(response.IsSuccessStatusCode, "Response was not successful");
            Assert.Equal(testTimeString, responseString);

            Assert.True(DateTimeOffset.TryParse(responseString, out var responseTime), "Response was not a valid DateTimeOffset");
            outputHelper.WriteLine($"ResponseTime: {responseTime}");

            Assert.Equal(testTime, responseTime, TimeSpan.Zero);
        }

        [Fact]
        public async Task TestOptions5558()
        {
            const int port = 5558;
            string expectedResponse = $"Hello from unit test {port}";

            var testTime = DateTimeOffset.UtcNow.AddHours(-10);

            outputHelper.WriteLine($"TestTime: {testTime}");

            var testTimeString = testTime.ToString("O");

            // Arrange
            using var iis = await fixture.StartAsync(port, outputHelper,
                ("AppOptions__TestResponse", expectedResponse),
                ("AppOptions__TestTime", testTimeString)
                );

            var client = fixture.CreateClient(port);

            // Act
            var response = await client.GetAsync("/options");
            var responseString = await response.Content.ReadAsStringAsync();

            outputHelper.WriteLine($"ResponseCode: {response.StatusCode}");
            outputHelper.WriteLine($"Response: {responseString}");

            // Assert
            Assert.True(response.IsSuccessStatusCode, "Response was not successful");
            
            var options = JsonSerializer.Deserialize<AppOptions>(responseString);
            Assert.NotNull(options);

            Assert.Equal(expectedResponse, options.TestResponse);
            Assert.True(options.UseTestTimeservice);
            Assert.NotNull(options.TestTime);
            Assert.Equal(testTime, options.TestTime.Value, TimeSpan.Zero);
        }
    }
}
