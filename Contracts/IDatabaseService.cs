using System.Collections.Generic;

namespace Codewars_Bot.Contracts
{
	public interface IDatabaseService
	{
		void AuditMessageInDatabase(string message);
		List<string> GetWeeklyRating(bool onlyActiveUsers);
		List<string> GetTotalRating();
		List<string> GetWeeklyPoints(int userId);
		string DeleteUserInfo(int userId);
		string SaveUserToDatabase(UserModel user);
		UserModel GetUserById(int userId);
	}
}
