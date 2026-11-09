using SOP_API.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SOP_API.Controllers
{
    [RoutePrefix("api/sop")]
    public class SOPController : ApiController
    {
        string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetSOP(int menuId)
        {
            var result = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        S.Id AS SectionId,
                        S.Name AS SectionName,
                        S.ImagePath,

                        G.Id AS GroupId,
                        G.Name AS GroupName,

                        D.Id AS DocId,
                        D.ItemTitle,
                        D.FilePath

                    FROM SOP_Sections S
                    LEFT JOIN SOP_Groups G ON G.SectionId = S.Id
                    LEFT JOIN SOP_Documents D ON D.GroupId = G.Id
                    WHERE S.MenuId = @MenuId
                    ORDER BY S.SortOrder, G.SortOrder";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MenuId", menuId);

                SqlDataReader reader = cmd.ExecuteReader();

                var sections = new Dictionary<int, dynamic>();

                while (reader.Read())
                {
                    int sectionId = (int)reader["SectionId"];

                    if (!sections.ContainsKey(sectionId))
                    {
                        sections[sectionId] = new
                        {
                            id = sectionId,
                            name = reader["SectionName"].ToString(),
                            image = reader["ImagePath"].ToString(),
                            groups = new Dictionary<int, dynamic>()
                        };
                    }

                    var section = sections[sectionId];

                    if (reader["GroupId"] != DBNull.Value)
                    {
                        int groupId = (int)reader["GroupId"];

                        if (!section.groups.ContainsKey(groupId))
                        {
                            section.groups[groupId] = new
                            {
                                id = groupId,
                                name = reader["GroupName"].ToString(),
                                items = new List<object>()
                            };
                        }

                        var group = section.groups[groupId];

                        if (reader["DocId"] != DBNull.Value)
                        {
                            group.items.Add(new
                            {
                                id = reader["DocId"],
                                title = reader["ItemTitle"].ToString(),
                                file = reader["FilePath"].ToString()
                            });
                        }
                    }
                }

                foreach (var s in sections.Values)
                {
                    result.Add(new
                    {
                        id = s.id,
                        name = s.name,
                        image = s.image,
                        groups = s.groups.Values
                    });
                }
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("section")]
        public IHttpActionResult GetSections(int menuId)
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"SELECT Id, Name, SortOrder FROM SOP_Sections WHERE MenuId=@MenuId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MenuId", menuId);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        SortOrder = (int)reader["SortOrder"]
                    });
                }
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("group")]
        public IHttpActionResult GetGroups(int sectionId)
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
            SELECT Id, Name 
            FROM SOP_Groups 
            WHERE SectionId = @SectionId
            ORDER BY Name
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString()
                    });
                }
            }

            return Ok(list);
        }

        [HttpPost]
        [Route("section")]
        public async Task<IHttpActionResult> PostSection()
        {
            var request = HttpContext.Current.Request;

            string name = request["Name"];
            int menuId = int.Parse(request["MenuId"]);

            string imagePath = "";

            if (request.Files.Count > 0)
            {
                var file = request.Files["image"];

                var saveInfo = FileStorageHelper.BuildSavePath("SOP", file.FileName);
                file.SaveAs(saveInfo.PhysicalPath);

                imagePath = saveInfo.RelativePath;   // contoh: "SOP/Image/nama.jpg"
            }

            string sortorder = request["SortOrder"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    INSERT INTO SOP_Sections (Name, MenuId, ImagePath, SortOrder)
                    VALUES (@Name, @MenuId, @ImagePath, @SortOrder)
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                cmd.Parameters.AddWithValue("@SortOrder", sortorder);

                cmd.ExecuteNonQuery();
            }

            return Ok("Section created");
        }

        [HttpPost]
        [Route("group")]
        public IHttpActionResult PostGroup([FromBody] dynamic data)
        {
            string name = data.Name;
            int sectionId = data.SectionId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
            INSERT INTO SOP_Groups (Name, SectionId)
            VALUES (@Name, @SectionId)
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);

                cmd.ExecuteNonQuery();
            }

            return Ok("Group created");
        }

        [HttpPost]
        [Route("upload")]
        public IHttpActionResult UploadSOP()
        {
            var httpRequest = HttpContext.Current.Request;

            string itemTitle = httpRequest["itemTitle"];
            int menuId = Convert.ToInt32(httpRequest["menuId"]);
            int sectionId = Convert.ToInt32(httpRequest["sectionId"]);
            int groupId = Convert.ToInt32(httpRequest["groupId"]);

            int? categoryId = null;
            if (!string.IsNullOrEmpty(httpRequest["categoryId"]))
                categoryId = null;

            string fileName = httpRequest["fileName"];
            string filePath = httpRequest["filePath"];
            string fileType = httpRequest["fileType"];
            string createdBy = httpRequest["createdBy"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
                INSERT INTO SOP_Documents
                (Title, FileName, FilePath, FileType,
                 MenuId, CategoryId, CreatedBy, CreatedDate,
                 GroupId, ItemTitle, SectionId)
                VALUES
                (@Title, @FileName, @FilePath, @FileType,
                 @MenuId, @CategoryId, @CreatedBy, GETDATE(),
                 @GroupId, @ItemTitle, @SectionId)";

                SqlCommand cmd = new SqlCommand(q, conn);

                cmd.Parameters.AddWithValue("@Title", itemTitle);
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.Parameters.AddWithValue("@FileType", fileType);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                cmd.Parameters.AddWithValue("@GroupId", groupId);
                cmd.Parameters.AddWithValue("@ItemTitle", itemTitle);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("section/{id}")]
        public IHttpActionResult DeleteSection(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "DELETE FROM SOP_Sections WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("group/{id}")]
        public IHttpActionResult DeleteGroup(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "DELETE FROM SOP_Groups WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("item/{id}")]
        public IHttpActionResult DeleteItem(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "DELETE FROM SOP_Documents WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        [Route("section/all")]
        public IHttpActionResult GetAllSections()
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
                    SELECT Id, Name, ImagePath, SortOrder
                    FROM SOP_Sections
                    ORDER BY SortOrder";

                SqlCommand cmd = new SqlCommand(q, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        ImagePath = reader["ImagePath"] == DBNull.Value ? "" : reader["ImagePath"].ToString(),
                        SortOrder = reader["SortOrder"] == DBNull.Value ? 0 : (int)reader["SortOrder"]
                    });
                }
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("group/all")]
        public IHttpActionResult GetAllGroups()
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
            SELECT 
                G.Id,
                G.Name,
                S.Name AS SectionName
            FROM SOP_Groups G
            LEFT JOIN SOP_Sections S ON S.Id = G.SectionId
            ORDER BY G.Name";

                SqlCommand cmd = new SqlCommand(q, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        SectionName = reader["SectionName"].ToString()
                    });
                }
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("item/all")]
        public IHttpActionResult GetAllItems()
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
            SELECT 
                D.Id,
                D.ItemTitle,
                D.FilePath,
                G.Name AS GroupName,
                S.Name AS SectionName
            FROM SOP_Documents D
            LEFT JOIN SOP_Groups G ON G.Id = D.GroupId
            LEFT JOIN SOP_Sections S ON S.Id = G.SectionId
            WHERE D.GroupId is not null and D.ItemTitle is not null
            ORDER BY D.CreatedDate DESC";

                SqlCommand cmd = new SqlCommand(q, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        ItemTitle = reader["ItemTitle"].ToString(),
                        FilePath = reader["FilePath"].ToString(),
                        GroupName = reader["GroupName"].ToString(),
                        SectionName = reader["SectionName"].ToString()
                    });
                }
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("category")]
        public IHttpActionResult GetCategories()
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = "SELECT Id, Name FROM SOP_Categories ORDER BY Name";

                SqlCommand cmd = new SqlCommand(q, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString()
                    });
                }
            }

            return Ok(list);
        }

        [HttpPut]
        [Route("item/{id}")]
        public IHttpActionResult UpdateItem(int id)
        {
            var httpRequest = HttpContext.Current.Request;

            string itemTitle = httpRequest["itemTitle"];
            int menuId = Convert.ToInt32(httpRequest["menuId"]);
            int sectionId = Convert.ToInt32(httpRequest["sectionId"]);
            int groupId = Convert.ToInt32(httpRequest["groupId"]);

            int? categoryId = null;
            if (!string.IsNullOrEmpty(httpRequest["categoryId"]))
                categoryId = Convert.ToInt32(httpRequest["categoryId"]);

            string newFileName = httpRequest["fileName"];
            string newFilePath = httpRequest["filePath"];
            string newFileType = httpRequest["fileType"];

            string createdBy = httpRequest["createdBy"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string oldFileName = "";
                string oldFilePath = "";
                string oldFileType = "";

                string getQuery = "SELECT FileName, FilePath, FileType FROM SOP_Documents WHERE Id=@Id";

                SqlCommand getCmd = new SqlCommand(getQuery, conn);
                getCmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = getCmd.ExecuteReader();

                if (reader.Read())
                {
                    oldFileName = reader["FileName"].ToString();
                    oldFilePath = reader["FilePath"].ToString();
                    oldFileType = reader["FileType"].ToString();
                }

                reader.Close();

                string finalFileName = !string.IsNullOrEmpty(newFileName) ? newFileName : oldFileName;
                string finalFilePath = !string.IsNullOrEmpty(newFilePath) ? newFilePath : oldFilePath;
                string finalFileType = !string.IsNullOrEmpty(newFileType) ? newFileType : oldFileType;

                string updateQuery = @"
                    UPDATE SOP_Documents
                    SET 
                        Title = @Title,
                        ItemTitle = @ItemTitle,
                        MenuId = @MenuId,
                        SectionId = @SectionId,
                        GroupId = @GroupId,
                        CategoryId = @CategoryId,
                        FileName = @FileName,
                        FilePath = @FilePath,
                        FileType = @FileType
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(updateQuery, conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", itemTitle);
                cmd.Parameters.AddWithValue("@ItemTitle", itemTitle);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@GroupId", groupId);
                cmd.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@FileName", finalFileName);
                cmd.Parameters.AddWithValue("@FilePath", finalFilePath);
                cmd.Parameters.AddWithValue("@FileType", finalFileType);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Item updated successfully" });
        }

        [HttpPut]
        [Route("section/{id}")]
        public IHttpActionResult UpdateSection(int id)
        {
            var httpRequest = HttpContext.Current.Request;

            string name = httpRequest["Name"];
            int menuId = Convert.ToInt32(httpRequest["MenuId"]);
            int sortorder = Convert.ToInt32(httpRequest["SortOrder"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // ambil ImagePath lama dulu, sebagai fallback kalau tidak ada file baru
                string oldImagePath = null;

                using (SqlCommand getCmd = new SqlCommand(
                    "SELECT ImagePath FROM SOP_Sections WHERE Id=@Id", conn))
                {
                    getCmd.Parameters.AddWithValue("@Id", id);
                    var result = getCmd.ExecuteScalar();
                    oldImagePath = (result == null || result == DBNull.Value) ? null : result.ToString();
                }

                string imagePath = oldImagePath;

                // 🔽 INI BAGIAN YANG KAMU TANYAKAN — taruh persis di sini 🔽
                if (httpRequest.Files.Count > 0 && httpRequest.Files["image"] != null
                    && httpRequest.Files["image"].ContentLength > 0)
                {
                    var file = httpRequest.Files["image"];
                    var saveInfo = FileStorageHelper.BuildSavePath("SOP", file.FileName);
                    file.SaveAs(saveInfo.PhysicalPath);

                    imagePath = saveInfo.RelativePath;
                }
                // 🔼 SAMPAI SINI 🔼

                string query = @"
            UPDATE SOP_Sections
            SET 
                Name = @Name,
                MenuId = @MenuId,
                ImagePath = @ImagePath,
                SortOrder = @SortOrder
            WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@ImagePath",
                    string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);
                cmd.Parameters.AddWithValue("@SortOrder", sortorder);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Section updated successfully" });
        }

        [HttpPut]
        [Route("group/{id}")]
        public IHttpActionResult UpdateGroup(int id)
        {
            var httpRequest = HttpContext.Current.Request;

            string name = httpRequest["name"];
            int sectionId = Convert.ToInt32(httpRequest["sectionId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
            UPDATE SOP_Groups
            SET 
                Name = @Name,
                SectionId = @SectionId
            WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@SectionId", sectionId);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Group updated successfully" });
        }

        [HttpGet]
        [Route("item/{id}")]
        public IHttpActionResult GetItemById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
            SELECT Id, ItemTitle, MenuId, SectionId, GroupId, CategoryId
            FROM SOP_Documents
            WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader r = cmd.ExecuteReader();

                if (!r.Read())
                    return NotFound();

                return Ok(new
                {
                    Id = r["Id"],
                    ItemTitle = r["ItemTitle"].ToString(),
                    MenuId = r["MenuId"],
                    SectionId = r["SectionId"],
                    GroupId = r["GroupId"],
                    CategoryId = r["CategoryId"] == DBNull.Value ? null : r["CategoryId"]
                });
            }
        }

        [HttpGet]
        [Route("section/{id}")]
        public IHttpActionResult GetSectionById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = @"
                    SELECT Id, MenuId, Name, ImagePath, SortOrder
                    FROM SOP_Sections
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader r = cmd.ExecuteReader();

                if (!r.Read())
                    return NotFound();

                return Ok(new
                {
                    Id = r["Id"],
                    MenuId = r["MenuId"],
                    Name = r["Name"],
                    ImagePath = r["ImagePath"] == DBNull.Value ? null : r["ImagePath"].ToString(),
                    SortOrder = r["SortOrder"] == DBNull.Value ? 0 : (int)r["SortOrder"]
                });
            }
        }

        [HttpGet]
        [Route("section/image/{id}")]
        public HttpResponseMessage GetSectionImage(int id)
        {
            string relativePath = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ImagePath FROM SOP_Sections WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                var result = cmd.ExecuteScalar();
                relativePath = (result == null || result == DBNull.Value) ? null : result.ToString();
            }

            if (string.IsNullOrEmpty(relativePath))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            string fullPath = FileStorageHelper.GetPhysicalPath(relativePath);

            if (fullPath == null || !File.Exists(fullPath))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(
                System.Web.MimeMapping.GetMimeMapping(fullPath));

            return response;
        }

        [HttpGet]
        [Route("group/{id}")]
        public IHttpActionResult GetGroupById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                SELECT G.Id, G.Name, S.MenuId, G.SectionId FROM SOP_Groups G
                left join SOP_Sections S on S.Id = G.SectionId
                WHERE G.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return NotFound();

                return Ok(new
                {
                    Id = reader["Id"],
                    Name = reader["Name"].ToString(),
                    MenuId = reader["MenuId"],
                    SectionId = reader["SectionId"]
                });
            }
        }

        [HttpPost]
        [Route("item/file")]
        public HttpResponseMessage UploadItemFile()
        {
            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Files.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "File tidak ditemukan");

            var file = httpRequest.Files[0];

            var saveInfo = FileStorageHelper.BuildSavePath("SOP", file.FileName);
            file.SaveAs(saveInfo.PhysicalPath);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                fileName = Path.GetFileName(saveInfo.PhysicalPath),
                filePath = saveInfo.RelativePath,
                fileType = Path.GetExtension(file.FileName)
            });
        }

        [HttpGet]
        [Route("item/download/{id}")]
        public HttpResponseMessage DownloadItemFile(int id, string mode = "download", string username = "")
        {
            bool isPreview = mode == "preview";

            if (isPreview)
            {
                // preview: cukup harus sudah login, tidak perlu DOWNLOAD_ACCESS
                if (string.IsNullOrEmpty(username))
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            else
            {
                // download: tetap wajib DOWNLOAD_ACCESS = 1
                if (!AccessHelper.HasDownloadAccess(username))
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            string fileName = null;
            string relativePath = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT FileName, FilePath FROM SOP_Documents WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                fileName = rd["FileName"].ToString();
                relativePath = rd["FilePath"].ToString();
            }

            string fullPath = FileStorageHelper.GetPhysicalPath(relativePath);
            if (fullPath == null || !File.Exists(fullPath))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(
                System.Web.MimeMapping.GetMimeMapping(fileName));

            string disposition = mode == "preview" ? "inline" : "attachment";
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(disposition)
            {
                FileName = fileName
            };

            return response;
        }
    }
}