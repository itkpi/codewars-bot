using System.Collections.Generic;

namespace Codewars_Bot.Contracts
{
	public interface IDatabaseService
	{
		void AuditMessageInDatabase(string message);
		List<string> GetWeeklyRating(int? numberOfUsersToDisplay = null);
		List<string> GetTotalRating();
		List<string> GetWeeklyPoints(int userId);
		string DeleteUserInfo(int userId);
		string SaveUserToDatabase(UserModel user);
		UserModel GetUserById(int userId);
	}
}
