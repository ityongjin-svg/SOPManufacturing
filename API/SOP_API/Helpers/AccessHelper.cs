using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SOP_API.Helpers
{
    public static class AccessHelper
    {
        public static bool HasDownloadAccess(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "SELECT DOWNLOAD_ACCESS FROM SOP_ACCESS WHERE USERNAME = @username";
                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@username", username);

                var result = cmd.ExecuteScalar();

                if (result == null || result == System.DBNull.Value)
                    return false;

                return (bool)result;
            }
        }

        public static bool HasQCManagementAccess(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "SELECT ACCESS FROM SOP_ACCESS WHERE USERNAME = @username";
                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@username", username);

                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return false;

                return (bool)result;
            }
        }
    }
}