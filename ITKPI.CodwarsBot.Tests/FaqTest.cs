using System.Linq;
using System.Threading.Tasks;
using Codewars_Bot.Contracts;
using FluentAssertions;
using ITKPI.CodwarsBot.Tests.Fixture;
using Microsoft.Bot.Connector;
using Xunit;

namespace ITKPI.CodwarsBot.Tests
{
    public class FaqTest : IClassFixture<IntegrationTestFixture>
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
