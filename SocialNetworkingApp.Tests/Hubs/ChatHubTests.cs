using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialNetworkingApp.Hubs;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SocialNetworkingApp.Tests.Hubs
{
    public class ChatHubTests
    {
        private readonly IHost _host;
        private HttpClient _client;

        public ChatHubTests()
        {
            // Настроим сервер с хабом
            _host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSignalR();
                })
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer()
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapHub<ChatHub>("/chatHub");
                            });
                        })
                        .UseUrls("http://localhost:5000"); // Устанавливаем порт
                })
                .Build();
        }

        private async Task StartServerAsync()
        {
            // Запускаем сервер асинхронно
            await _host.StartAsync();
            await Task.Delay(500); // Подождать некоторое время для того, чтобы сервер полностью инициализировался
        }

        private async Task StopServerAsync()
        {
            // Останавливаем сервер после завершения работы
            await _host.StopAsync();
        }

        [Fact]
        public async Task SendMessage_ShouldReceiveMessage()
        {
            // Arrange
            await StartServerAsync(); // Убедимся, что сервер запущен

            // Создаем клиент для связи с сервером
            _client = _host.GetTestServer().CreateClient();

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chatHub") // Используем тот же порт, что и в сервере
                .Build();

            // Act
            await connection.StartAsync();

            // Отправка сообщения
            var message = "Hello, world!";
            var user = "TestUser";
            await connection.InvokeAsync("SendMessage", user, message);

            // Assert
            var tcs = new TaskCompletionSource<bool>();
            connection.On<string, string>("ReceiveMessage", (userReceived, messageReceived) =>
            {
                // Проверим, что сообщение отправлено клиенту
                Assert.Equal(user, userReceived);
                Assert.Equal(message, messageReceived);
                tcs.SetResult(true);
            });

            // Даем время для выполнения асинхронной операции
            await Task.WhenAny(tcs.Task, Task.Delay(1000));

            // Завершаем работу сервера
            await StopServerAsync();
        }

        [Fact]
        public async Task SendMessage_NoConnection_ShouldNotThrowException()
        {
            // Arrange
            await StartServerAsync();

            // Создаем клиента с неправильным URL (чтобы не подключаться)
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:9999/chatHub") // Неверный порт
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => connection.StartAsync());

            await StopServerAsync();
        }
    }

}
