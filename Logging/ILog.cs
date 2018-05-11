using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codewars_Bot.Logging
{
    public interface ILog
    {
        void Info(string message);
        void Error(string message);
        void Error(Exception exception, string message = "");
    }
}