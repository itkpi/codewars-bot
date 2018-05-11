using System.Collections.Generic;
using Codewars_Bot.Models;

namespace Codewars_Bot.Contracts
{
	public interface IDatabaseService
	{
		WeekModel GetLastWeek();
		List<UserModel> GetWeeklyRating(WeekModel week);
		List<UserModel> GetTotalRating();
		List<WeeklyPointsModel> GetWeeklyPoints(int userId);
		bool DeleteUserInfo(int userId);
		bool SaveUserToDatabase(UserModel user);
		UserModel GetUserById(int userId);
	}
}
