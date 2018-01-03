using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codewars_Bot.Models
{
    public class WeeklyRatingUserModel
    {
        public int Id { get; set; }
        public string CodewarsUsername { get; set; }
        public int WeekNumber { get; set; }
        public int Points { get; set; }
    }
}