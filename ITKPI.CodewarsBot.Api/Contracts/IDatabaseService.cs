using System.Collections.Generic;
using ITKPI.CodewarsBot.Api.Models;

namespace ITKPI.CodewarsBot.Api.Contracts
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
