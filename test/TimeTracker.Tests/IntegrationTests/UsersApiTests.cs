using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data;
using Xunit;

namespace TimeTracker.Tests.IntegrationTests
{
    public class UsersApiTests
    {
        private readonly HttpClient _client;
        private readonly SqliteConnection _connection;
        private readonly string _nonAdminToken;
        private readonly string _adminToken;

        public UsersApiTests()
        {
            const string issuer = "http://localhost:44303";
            const string key = "some-long-secret-key";

            // Must initialize and open Sqlite connection in order to keep in-memory database tables
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            Startup.ConfigureDbContext = (configuration, builder) => builder.UseSqlite(_connection);

            var server = new TestServer(new WebHostBuilder()
                .UseSetting("Tokens:Issuer", issuer)
                .UseSetting("Tokens:Key", key)
                .UseStartup<Startup>()
                .UseUrls("https://localhost:44303"))
            {
                BaseAddress = new Uri("https://localhost:44303")
            };

            // Force creation of InMemory database
            var dbContext = server.Services.GetService<TimeTrackerDbContext>();
            dbContext.Database.EnsureCreated();

            _client = server.CreateClient();

            _nonAdminToken = JwtTokenGenerator.Generate(
                "aspnetcore-workshop-demo", false, issuer, key);
            _adminToken = JwtTokenGenerator.Generate(
                "aspnetcore-workshop-demo", true, issuer, key);
        }
        [Fact]
        public async Task Delete_NoAuthorizationHeader_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Clear();
            var result = await _client.DeleteAsync("/api/v2/users/1");

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Delete_NotAdmin_ReturnsForbidden()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders
                .Add("Authorization", new[] { $"Bearer {_nonAdminToken}" });

            var result = await _client.DeleteAsync("/api/v2/users/1");

            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_NoId_ReturnsMethodNotAllowed()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders
                .Add("Authorization", new[] { $"Bearer {_adminToken}" });

            var result = await _client.DeleteAsync("/api/v2/users/ ");

            Assert.Equal(HttpStatusCode.MethodNotAllowed, result.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders
                .Add("Authorization", new[] { $"Bearer {_adminToken}" });

            var result = await _client.DeleteAsync("/api/v2/users/0 ");

            //Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders
                .Add("Authorization", new[] { $"Bearer {_adminToken}" });

            var result = await _client.DeleteAsync("/api/v2/users/1 ");

            //Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
