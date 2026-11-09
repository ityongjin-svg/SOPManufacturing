using SOP_API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SOP_API.Controllers
{

    public class AuthController : ApiController
    {
        string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpPost]
        [Route("api/login")]
        public IHttpActionResult Login(LoginRequest model)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("BSM_LOGIN_WEB_CHECK", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@username", model.Username);
                cmd.Parameters.AddWithValue("@pwd", model.Password);

                SqlDataReader reader = cmd.ExecuteReader();
                bool loginValid = reader.HasRows;
                reader.Close();

                if (!loginValid)
                {
                    return Ok(new
                    {
                        success = false
                    });
                }

                bool access = false;
                bool accessSuper = false;

                SqlCommand accessCmd = new SqlCommand(
                    "SELECT ACCESS, ACCESS_SUPER FROM SOP_ACCESS WHERE USERNAME = @Username",
                    conn);
                accessCmd.Parameters.AddWithValue("@Username", model.Username);

                SqlDataReader accessReader = accessCmd.ExecuteReader();

                if (accessReader.Read())
                {
                    access = accessReader["ACCESS"] != DBNull.Value
                        && Convert.ToInt32(accessReader["ACCESS"]) == 1;

                    accessSuper = accessReader["ACCESS_SUPER"] != DBNull.Value
                        && Convert.ToInt32(accessReader["ACCESS_SUPER"]) == 1;
                }

                accessReader.Close();

                // Kalau username tidak terdaftar sama sekali di SOP_ACCESS,
                // access & accessSuper otomatis false (default paling aman).
                return Ok(new
                {
                    success = true,
                    username = model.Username,
                    access = access,
                    accessSuper = accessSuper
                });
            }
        }
    }
}
