namespace ITKPI.CodewarsBot.Api.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string CodewarsUsername { get; set; }
        public string CodewarsFullname { get; set; }
        public string TelegramUsername { get; set; }
		public int TelegramId { get; set; }
        public int Points { get; set; }
    }
}
