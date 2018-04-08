using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codewars_Bot.Contracts
{
	public interface IDatabaseService
	{
		void AuditMessageInDatabase(string message);
		List<string> GetWeeklyRating();
		List<string> GetTotalRating();
		List<string> GetWeeklyPoints(int userId);
		string DeleteUserInfo(int userId);
		string SaveUserToDatabase(UserModel user);
		UserModel GetUserById(int userId);
	}
}
