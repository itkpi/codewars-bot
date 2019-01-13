using System;

namespace ITKPI.CodewarsBot.Api.Models
{
	public class WeeklyPointsModel
	{
		public int WeekNumber { get; set; }
		public DateTime EndDate { get; set; }
		public int Points { get; set; }
	}
}
