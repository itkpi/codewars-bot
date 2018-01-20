using System;

namespace Codewars_Bot.Models
{
	public class WeeklyPointsModel
	{
		public int WeekNumber { get; set; }
		public DateTime EndDate { get; set; }
		public int Points { get; set; }
	}
}