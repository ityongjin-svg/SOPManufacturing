using SOP_API.Helpers;
using SOP_API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace SOP_API.Controllers
{
    [RoutePrefix("api/machine")]
    public class MachineController : ApiController
    {
        string cs = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetMachine(int categoryId)
        {
            var result = new List<MachineItemDTO>();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        i.Id AS ItemId, i.ItemName,
                        t.Id AS TypeId, t.TypeName,
                        f.Id AS FileId, f.FileName, f.FilePath
                    FROM SOP_Machine_Items i
                    LEFT JOIN SOP_Machine_Types t ON t.ItemId = i.Id
                    LEFT JOIN SOP_Machine_Files f ON f.TypeId = t.Id
                    WHERE i.CategoryId = @CategoryId AND i.IsActive = 1
                    ORDER BY i.SortOrder, t.SortOrder";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                SqlDataReader reader = cmd.ExecuteReader();

                var itemLookup = new Dictionary<int, MachineItemDTO>();
                var typeLookup = new Dictionary<int, MachineTypeDTO>();

                while (reader.Read())
                {
                    int itemId = (int)reader["ItemId"];
                    int typeId = reader["TypeId"] == DBNull.Value ? 0 : (int)reader["TypeId"];

                    // ITEM
                    if (!itemLookup.ContainsKey(itemId))
                    {
                        itemLookup[itemId] = new MachineItemDTO
                        {
                            ItemName = reader["ItemName"].ToString(),
                            Types = new List<MachineTypeDTO>()
                        };
                    }

                    var item = itemLookup[itemId];

                    // TYPE
                    if (typeId != 0 && !typeLookup.ContainsKey(typeId))
                    {
                        var type = new MachineTypeDTO
                        {
                            Id = typeId,          // tambahkan ini
                            TypeName = reader["TypeName"].ToString(),
                            Files = new List<MachineFileDTO>()
                        };

                        typeLookup[typeId] = type;
                        item.Types.Add(type);
                    }

                    // FILE
                    if (typeId != 0 && reader["FileId"] != DBNull.Value)
                    {
                        typeLookup[typeId].Files.Add(new MachineFileDTO
                        {
                            Id = (int)reader["FileId"],
                            FileName = reader["FileName"].ToString(),
                            FilePath = reader["FilePath"].ToString()
                        });
                    }
                }

                result = itemLookup.Values.ToList();
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("download/{id}")]
        public HttpResponseMessage DownloadFile(int id, string mode = "download", string username = "")
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

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand("SELECT FileName, FilePath FROM SOP_Machine_Files WHERE Id=@id", conn);
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

        [HttpPost]
        [Route("upload")]
        public IHttpActionResult Upload()
        {
            var httpRequest = System.Web.HttpContext.Current.Request;

            string itemName = httpRequest["ItemName"];
            string typeName = httpRequest["TypeName"];
            int categoryId = int.Parse(httpRequest["CategoryId"]);

            var file = httpRequest.Files[0];

            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(typeName) || file == null)
                return BadRequest("Data tidak lengkap");

            int itemId = 0;
            int typeId = 0;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                // ======================
                // GET / INSERT ITEM
                // ======================
                var cmd = new SqlCommand("SELECT Id FROM SOP_Machine_Items WHERE ItemName = @name AND CategoryId = @cat", conn);
                cmd.Parameters.AddWithValue("@name", itemName);
                cmd.Parameters.AddWithValue("@cat", categoryId);

                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    itemId = Convert.ToInt32(result);
                }
                else
                {
                    cmd = new SqlCommand(@"
                INSERT INTO SOP_Machine_Items (ItemName, CategoryId, SortOrder, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@name, @cat, 0, 1)", conn);

                    cmd.Parameters.AddWithValue("@name", itemName);
                    cmd.Parameters.AddWithValue("@cat", categoryId);

                    itemId = (int)cmd.ExecuteScalar();
                }

                // ======================
                // GET / INSERT TYPE
                // ======================
                cmd = new SqlCommand("SELECT Id FROM SOP_Machine_Types WHERE TypeName = @name AND ItemId = @itemId", conn);
                cmd.Parameters.AddWithValue("@name", typeName);
                cmd.Parameters.AddWithValue("@itemId", itemId);

                result = cmd.ExecuteScalar();

                if (result != null)
                {
                    typeId = Convert.ToInt32(result);
                }
                else
                {
                    cmd = new SqlCommand(@"
                        INSERT INTO SOP_Machine_Types (TypeName, ItemId, SortOrder, IsActive)
                        OUTPUT INSERTED.Id
                        VALUES (@name, @itemId, 0, 1)", conn);

                    cmd.Parameters.AddWithValue("@name", typeName);
                    cmd.Parameters.AddWithValue("@itemId", itemId);

                    typeId = (int)cmd.ExecuteScalar();
                }

                // ======================
                // SAVE FILE (D:\SOP\Document\{Tipe}\...)
                // ======================
                var saveInfo = FileStorageHelper.BuildSavePath("Machine", file.FileName);
                string fileName = System.IO.Path.GetFileName(saveInfo.PhysicalPath);

                file.SaveAs(saveInfo.PhysicalPath);

                // ======================
                // INSERT FILE
                // ======================
                cmd = new SqlCommand(@"
                    INSERT INTO SOP_Machine_Files (TypeId, FileName, FilePath)
                    VALUES (@typeId, @fileName, @filePath)", conn);

                cmd.Parameters.AddWithValue("@typeId", typeId);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@filePath", saveInfo.RelativePath);

                cmd.ExecuteNonQuery();
            }

            return Ok("Upload berhasil");
        }

        [HttpDelete]
        [Route("file/{id}")]
        public IHttpActionResult DeleteFile(int id)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                // =========================
                // 1. Ambil FilePath dulu
                // =========================
                string filePath = "";

                var cmdGet = new SqlCommand(
                    "SELECT FilePath FROM SOP_Machine_Files WHERE Id=@id", conn);

                cmdGet.Parameters.AddWithValue("@id", id);

                var result = cmdGet.ExecuteScalar();

                if (result == null)
                    return NotFound();

                filePath = result.ToString();

                // =========================
                // 2. Hapus file fisik
                // =========================
                try
                {
                    string fullPath = FileStorageHelper.GetPhysicalPath(filePath);

                    if (fullPath != null && System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    // optional: log error
                }

                // =========================
                // 3. Hapus dari DB
                // =========================
                var cmdDelete = new SqlCommand(
                    "DELETE FROM SOP_Machine_Files WHERE Id=@id", conn);

                cmdDelete.Parameters.AddWithValue("@id", id);
                cmdDelete.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPost]
        [Route("update")]
        public IHttpActionResult UpdateMachine()
        {
            var request = HttpContext.Current.Request;

            int id = Convert.ToInt32(request["Id"]);
            int typeId = Convert.ToInt32(request["TypeId"]);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                    UPDATE SOP_Machine_Files
                    SET TypeId = @typeId
                    WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@typeId", typeId);
                cmd.ExecuteNonQuery();

                if (request.Files.Count > 0)
                {
                    var file = request.Files[0];
                    var saveInfo = FileStorageHelper.BuildSavePath("Machine", file.FileName);
                    string fileName = Path.GetFileName(saveInfo.PhysicalPath);
                    file.SaveAs(saveInfo.PhysicalPath);

                    cmd = new SqlCommand(@"
                        UPDATE SOP_Machine_Files
                        SET FileName = @fileName, FilePath = @filePath
                        WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@fileName", fileName);
                    cmd.Parameters.AddWithValue("@filePath", saveInfo.RelativePath);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Updated");
        }

        [HttpGet]
        [Route("items")]
        public IHttpActionResult GetItems(int categoryId)
        {
            var list = new List<Machine_Items>();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT Id, ItemName, SortOrder, IsActive 
            FROM SOP_Machine_Items 
            WHERE CategoryId = @cat AND IsActive = 1
            ORDER BY SortOrder", conn);

                cmd.Parameters.AddWithValue("@cat", categoryId);

                var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    list.Add(new Machine_Items
                    {
                        Id = (int)rd["Id"],
                        ItemName = rd["ItemName"].ToString(),
                        SortOrder = rd["SortOrder"] == DBNull.Value ? 0 : (int)rd["SortOrder"],
                        IsActive = (bool)rd["IsActive"]
                    });
                }
            }

            return Ok(list);
        }

        [HttpPost]
        [Route("item")]
        public IHttpActionResult InsertItem(Machine_Items model)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            INSERT INTO SOP_Machine_Items (CategoryId, ItemName, SortOrder, IsActive)
            VALUES (@cat, @name, @sort, 1)", conn);

                cmd.Parameters.AddWithValue("@cat", model.CategoryId);
                cmd.Parameters.AddWithValue("@name", model.ItemName);
                cmd.Parameters.AddWithValue("@sort", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPut]
        [Route("item")]
        public IHttpActionResult UpdateItem(Machine_Items model)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            UPDATE SOP_Machine_Items 
            SET ItemName=@name, SortOrder=@sort 
            WHERE Id=@id", conn);

                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@name", model.ItemName);
                cmd.Parameters.AddWithValue("@sort", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("item/{id}")]
        public IHttpActionResult DeleteItem(int id)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            UPDATE SOP_Machine_Items 
            SET IsActive = 0 
            WHERE Id=@id", conn);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        [Route("types")]
        public IHttpActionResult GetTypes(int itemId)
        {
            var list = new List<Machine_Types>();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT Id, ItemId, TypeName, SortOrder, IsActive
            FROM SOP_Machine_Types
            WHERE ItemId = @itemId AND IsActive = 1
            ORDER BY SortOrder", conn);

                cmd.Parameters.AddWithValue("@itemId", itemId);

                var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    list.Add(new Machine_Types
                    {
                        Id = (int)rd["Id"],
                        ItemId = (int)rd["ItemId"],
                        TypeName = rd["TypeName"].ToString(),
                        SortOrder = rd["SortOrder"] == DBNull.Value ? 0 : (int)rd["SortOrder"],
                        IsActive = (bool)rd["IsActive"]
                    });
                }
            }

            return Ok(list);
        }

        [HttpPost]
        [Route("type")]
        public IHttpActionResult InsertType(Machine_Types model)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            INSERT INTO SOP_Machine_Types (ItemId, TypeName, SortOrder, IsActive)
            VALUES (@itemId, @name, @sort, 1)", conn);

                cmd.Parameters.AddWithValue("@itemId", model.ItemId);
                cmd.Parameters.AddWithValue("@name", model.TypeName);
                cmd.Parameters.AddWithValue("@sort", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPut]
        [Route("type")]
        public IHttpActionResult UpdateType(Machine_Types model)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            UPDATE SOP_Machine_Types
            SET TypeName=@name, SortOrder=@sort
            WHERE Id=@id", conn);

                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@name", model.TypeName);
                cmd.Parameters.AddWithValue("@sort", model.SortOrder);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("type/{id}")]
        public IHttpActionResult DeleteType(int id)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(
                    "DELETE FROM SOP_Machine_Types WHERE Id=@id", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet]
        [Route("master/files")]
        public IHttpActionResult GetMasterFiles()
        {
            var list = new List<MachineFile>();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT Id, FileName, FilePath, FileIcon, UploadedAt
            FROM SOP_Machine_Files
            WHERE TypeId IS NULL
            ORDER BY UploadedAt DESC", conn);

                var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    list.Add(new MachineFile
                    {
                        Id = (int)rd["Id"],
                        FileName = rd["FileName"].ToString(),
                        FilePath = rd["FilePath"].ToString(),
                        FileIcon = rd["FileIcon"]?.ToString(),
                        UploadedAt = Convert.ToDateTime(rd["UploadedAt"])
                    });
                }
            }

            return Ok(list);
        }

        [HttpPost]
        [Route("master/file")]
        public IHttpActionResult UploadMasterFile()
        {
            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Files.Count == 0)
                return BadRequest("File tidak ditemukan");

            var file = httpRequest.Files[0];

            // API sendiri yang menyimpan fisiknya ke D:\SOP\Document\{Tipe}\
            // (sebelumnya file ini disimpan oleh project UI di foldernya sendiri,
            // yang bikin lokasi file tidak konsisten dengan file machine biasa)
            var saveInfo = FileStorageHelper.BuildSavePath("Machine", file.FileName);
            string fileName = Path.GetFileName(saveInfo.PhysicalPath);

            file.SaveAs(saveInfo.PhysicalPath);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            INSERT INTO SOP_Machine_Files
            (FileName, FilePath, FileIcon, UploadedAt, TypeId)
            VALUES (@name, @path, '', GETDATE(), NULL)", conn);

                cmd.Parameters.AddWithValue("@name", fileName);
                cmd.Parameters.AddWithValue("@path", saveInfo.RelativePath);

                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("master/file/{id}")]
        public IHttpActionResult DeleteMasterFile(int id)
        {
            string filePath = "";

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                // ambil file path dulu
                var cmd = new SqlCommand("SELECT FilePath FROM SOP_Machine_Files WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                filePath = cmd.ExecuteScalar()?.ToString();

                // delete DB
                cmd = new SqlCommand("DELETE FROM SOP_Machine_Files WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // delete file fisik
            if (!string.IsNullOrEmpty(filePath))
            {
                string fullPath = FileStorageHelper.GetPhysicalPath(filePath);

                if (fullPath != null && File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            return Ok();
        }

        [HttpGet]
        [Route("master/download")]
        public IHttpActionResult DownloadMasterFile()
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT TOP 1 Id, FileName, FilePath
            FROM SOP_Machine_Files
            WHERE TypeId IS NULL
            ORDER BY UploadedAt DESC", conn);

                var rd = cmd.ExecuteReader();

                if (!rd.Read())
                    return NotFound();

                int id = (int)rd["Id"];
                string fileName = rd["FileName"].ToString();

                return Ok(new
                {
                    Id = id,
                    FileName = fileName
                });
            }
        }
    }
}