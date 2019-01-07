﻿using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codewars_Bot.Contracts
{
	public interface IMessageService
	{
		Task<List<string>> ProcessMessage(Activity activity);
	}
}
