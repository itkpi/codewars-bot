using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Codewars_Bot.Models;
using Dapper;

namespace Codewars_Bot.Logging
{
    public class DbLog : ILog
    {
        public void Info(string message)
        {
            var formatted = $"INFO: {message}";
            Log(formatted);
        }

        public void Error(Exception exception, string message = "")
        {
            Error($"{exception.Message} {exception.StackTrace}, {message}");
        }

        public void Error(string message)
        {
            var formatted = $"EXCEPTION: {message}";
            Log(formatted);
        }

        private void Log(string message)
        {
            using (var connection = new SqlConnection(Configuration.DbConnection))
            {
                var query = $"INSERT INTO [Audit].[Messages] (Message, DateTime) VALUES (@Message, GETDATE())";
                connection.Query(query, new AuditMessageModel { Message = message });
            }
        }
    }
}