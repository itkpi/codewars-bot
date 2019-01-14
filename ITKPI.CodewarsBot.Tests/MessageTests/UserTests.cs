using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Api.DataAccess;
using ITKPI.CodewarsBot.Api.Models;
using ITKPI.CodewarsBot.Tests.Fixture;
using Microsoft.Bot.Schema;
using Xunit;

namespace ITKPI.CodewarsBot.Tests.MessageTests
{
    public class UserTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IMessageService _sut;
        private readonly IUsersRepository _usersRepository;

        public UserTests(IntegrationTestFixture fixture)
        {
            _sut = fixture.ResolveDependency<IMessageService>();
            _usersRepository = fixture.ResolveDependency<IUsersRepository>();
        }

        [Fact]
        public async Task FirstCommand_RegistersUser()
        {

            var result = await _sut.ProcessMessage(new Activity()
            {
                From = new ChannelAccount("12345", "SomeName"),
                Text = "codewars_login123",
                Conversation = new ConversationAccount(false)
            });

            result.Single().Should().StartWith("Реєстрація успішна!");

            var user = await _usersRepository.Find(12345);
            user.Should().NotBeNull();
            user.Should().BeEquivalentTo(new UserModel
            {
                TelegramUsername = "SomeName",
                CodewarsUsername = "codewars_login123",
                Points = 9911,
                CodewarsFullname = "SomeCodewarsName",
                TelegramId = 12345
            });
        }

        [Fact]
        public async Task DeleteUserInfo_DeletesUser()
        {
            var id = 123456;
            await _sut.ProcessMessage(new Activity()
            {
                From = new ChannelAccount(id.ToString(), "SomeName"),
                Text = "codewars_login123",
                Conversation = new ConversationAccount(false)
            });

            var result = await _sut.ProcessMessage(new Activity()
            {
                From = new ChannelAccount(id.ToString()),
                Text = "/delete_userinfo"
            });

            result.Single().Should().Be("Видалення пройшло успішно");

            var user = await _usersRepository.Find(id);
            user.Should().BeNull();
        }

        [Fact]
        public async Task CannotRegisterSecondTime()
        {
            var id = 123098;
            await _sut.ProcessMessage(new Activity()
            {
                From = new ChannelAccount(id.ToString(), "SomeName"),
                Text = "codewars_login123",
                Conversation = new ConversationAccount(false)
            });
            var response = await _sut.ProcessMessage(new Activity()
            {
                From = new ChannelAccount(id.ToString(), "SomeName"),
                Text = "codewars_otherlogin",
                Conversation = new ConversationAccount(false)
            });

            response.Single().Should().Be($"Ви вже зареєстровані в рейтингу Codewars під ніком codewars_login123");

            var user = await _usersRepository.Find(id);
            user.CodewarsUsername.Should().Be("codewars_login123");
        }
    }
}
