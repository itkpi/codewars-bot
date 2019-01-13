namespace ITKPI.CodewarsBot.Api.Models
{
    public class WeeklyRatingUserModel
    {
        public int Id { get; set; }
        public string CodewarsUsername { get; set; }
        public int WeekNumber { get; set; }
        public int Points { get; set; }
    }
}
