using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ITKPI.CodewarsBot.Api.Contracts;
using ITKPI.CodewarsBot.Tests.Fixture;
using Microsoft.Bot.Schema;
using Xunit;

namespace ITKPI.CodewarsBot.Tests.MessageTests
{
    [Collection("IntegrationTests")]
    public class FaqTest
    {
        private readonly IMessageService _sut;

        public FaqTest(IntegrationTestFixture fixture)
        {
            _sut = fixture.ResolveDependency<IMessageService>();
        }

        [Fact]
        public async Task ShouldShowFaq_WhenCommandIsFaq()
        {
            var result = await _sut.ProcessMessage(new Activity
            {
                Text = "/show_faq",
                From = new ChannelAccount()
            });

            result.Count.Should().Be(1);

            result.First().Should().StartWith("Вітаємо в клані ІТ КРІ на Codewars!");
        }
    }
}
