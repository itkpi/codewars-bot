using System.Text.RegularExpressions;
using Codewars_Bot.Infrastructure;

namespace Codewars_Bot
{
    public class UserModel
    {
        public int TelegramId { get; set; }

        public string TelegramUsername { get; set; }

        public string CodewarsUsername { get; set; }

        public string CodewarsFullname { get; set; }

        public int Points { get; set; }


        private static readonly Regex ValidationRegex = new Regex(@"^[a-zA-Z0-9\s_.-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsValidCodewarsUserName(string userName)
        {
            return ValidationRegex.IsMatch(userName);
        }

        public static UserModel Create(int telegramId, string telegramUserName, string codewarsUserName, string codewarsFullName, int points)
        {
            return new UserModel
            {
                TelegramId = telegramId,
                TelegramUsername = telegramUserName,
                CodewarsUsername = codewarsUserName,
                CodewarsFullname = codewarsFullName,
                Points = points
            };
        }
    }
}
