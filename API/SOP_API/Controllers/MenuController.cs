using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Data.SqlClient;
using System.Configuration;
using SOP_API.Models;
using System;

namespace SOP_API.Controllers
{
    public class MenuController : ApiController
    {
        string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        [Route("api/menu")]
        public IHttpActionResult GetMenus()
        {
            List<Menu> menus = new List<Menu>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"
                    ; WITH MenuCTE AS(
                         SELECT
                             Id,Name AS ParentName,Name, NameKr,ParentId,Level,Url,SortOrder,CAST(RIGHT('000' + CAST(SortOrder AS VARCHAR), 3) AS VARCHAR(MAX)) AS SortPath
                         FROM SOP_Menus
                         WHERE ParentId IS NULL
 
                         UNION ALL
 
                         SELECT
                             m.Id,cte.Name AS ParentName,m.Name, m.NameKr,m.ParentId,m.Level,m.Url,m.SortOrder,CAST(cte.SortPath + '-' + RIGHT('000' + CAST(m.SortOrder AS VARCHAR), 3) AS VARCHAR(MAX)) 
                         FROM SOP_Menus m
                         INNER JOIN MenuCTE cte ON m.ParentId = cte.Id
                     )
                    SELECT *
                    FROM MenuCTE
                    ORDER BY SortPath;
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    menus.Add(new Menu
                    {
                        Id = (int)reader["Id"],
                        ParentName = reader["ParentName"].ToString(),
                        Name = reader["Name"].ToString(),
                        NameKr = reader["NameKr"].ToString(),
                        ParentId = reader["ParentId"] == DBNull.Value ? (int?)null : (int)reader["ParentId"],
                        Level = (int)reader["Level"],
                        Url = reader["Url"]?.ToString()
                    });
                }
            }

            //var tree = BuildTree(menus, null);

            return Ok(menus);
        }

        private List<object> BuildTree(List<Menu> menus, int? parentId)
        {
            return menus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    url = m.Url,
                    hasChildren = menus.Any(x => x.ParentId == m.Id),
                    children = BuildTree(menus, m.Id)
                }).ToList<object>();
        }

        [HttpPost]
        [Route("api/menu")]
        public IHttpActionResult InsertMenu(Menu model)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"INSERT INTO SOP_Menus (Name, ParentId, Level, Url, SortOrder, IsActive)
                         VALUES (@Name, @ParentId, @Level, @Url, @SortOrder, 1)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@ParentId", (object)model.ParentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Level", model.Level);
                cmd.Parameters.AddWithValue("@Url", model.Url);
                cmd.Parameters.AddWithValue("@SortOrder", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPut]
        [Route("api/menu/{id}")]
        public IHttpActionResult UpdateMenu(int id, Menu model)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"UPDATE SOP_Menus 
                         SET Name=@Name, ParentId=@ParentId, Level=@Level, Url=@Url, SortOrder=@SortOrder
                         WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@ParentId", (object)model.ParentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Level", model.Level);
                cmd.Parameters.AddWithValue("@Url", model.Url);
                cmd.Parameters.AddWithValue("@SortOrder", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("api/menu/{id}")]
        public IHttpActionResult DeleteMenu(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM SOP_Menus WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/menu/{id}")]
        public IHttpActionResult GetMenuById(int id)
        {
            Menu menu = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"SELECT Id, Name, ParentId, Level, Url, SortOrder 
                         FROM SOP_Menus 
                         WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    menu = new Menu
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        ParentId = reader["ParentId"] == DBNull.Value ? (int?)null : (int)reader["ParentId"],
                        Level = (int)reader["Level"],
                        Url = reader["Url"]?.ToString(),
                        SortOrder = reader["SortOrder"] == DBNull.Value ? 0 : (int)reader["SortOrder"]
                    };
                }
            }

            if (menu == null)
                return NotFound();

            return Ok(menu);
        }
    }
}