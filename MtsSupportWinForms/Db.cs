using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MtsSupportWinForms
{
    public static class Db
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["MTS_SUPPORT2"].ConnectionString; }
        }

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var table = new DataTable();
                connection.Open();
                adapter.Fill(table);
                return table;
            }
        }

        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                var rows = command.ExecuteNonQuery();
                LogService.Log("Изменение данных", ShortSql(sql));
                return rows;
            }
        }

        public static object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public static int NextId(string tableName, string idField)
        {
            var value = Scalar(string.Format("SELECT ISNULL(MAX({0}), 0) + 1 FROM {1}", idField, tableName));
            return Convert.ToInt32(value);
        }

        public static bool CanConnect(out string error)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    error = string.Empty;
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static int Count(string sql, params SqlParameter[] parameters)
        {
            return Convert.ToInt32(Scalar(sql, parameters));
        }

        private static string ShortSql(string sql)
        {
            var text = (sql ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            if (text.Length > 120) text = text.Substring(0, 120) + "...";
            return text;
        }
    }
}
