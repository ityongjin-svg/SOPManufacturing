using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Data.SqlClient;
using System.Configuration;
using SOP_API.Models;
using SOP_API.Helpers;
//using PowerPoint = Microsoft.Office.Interop.PowerPoint;
//using Microsoft.Office.Core;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace SOP_API.Controllers
{
    public class DocumentController : ApiController
    {
        string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public void ConvertPptToPdf(string inputPath, string outputPath)
        {
            string sofficePath = ConfigurationManager.AppSettings["LibreOfficePath"]
                ?? @"C:\Program Files\LibreOffice\program\soffice.exe";

            if (!File.Exists(sofficePath))
                throw new Exception($"LibreOffice tidak ditemukan di '{sofficePath}'. Install LibreOffice di server, atau set path yang benar lewat appSettings 'LibreOfficePath' di Web.config.");

            // Convert ke folder temp dulu (bukan langsung ke folder tujuan), supaya
            // tidak mungkin menimpa file lain yang kebetulan sama nama dasarnya
            // di folder PDF. LibreOffice selalu menamai hasil convert sesuai nama
            // file INPUT, bukan nama yang kita mau — makanya perlu di-rename dulu.
            string tempOutDir = Path.Combine(Path.GetTempPath(), "lo_out_" + Guid.NewGuid());
            Directory.CreateDirectory(tempOutDir);

            // Profile LibreOffice terpisah per proses, supaya kalau ada beberapa
            // upload bersamaan, mereka tidak rebutan lock file profile yang sama.
            string userProfileDir = Path.Combine(Path.GetTempPath(), "lo_profile_" + Guid.NewGuid());

            string expectedOutputName = Path.GetFileNameWithoutExtension(inputPath) + ".pdf";
            string expectedOutputPath = Path.Combine(tempOutDir, expectedOutputName);

            var psi = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = $"--headless --norestore -env:UserInstallation=\"file:///{userProfileDir.Replace('\\', '/')}\" --convert-to pdf --outdir \"{tempOutDir}\" \"{inputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    string stdOut = process.StandardOutput.ReadToEnd();
                    string stdErr = process.StandardError.ReadToEnd();

                    bool finished = process.WaitForExit(120000); // timeout 2 menit

                    if (!finished)
                    {
                        try { process.Kill(); } catch { }
                        throw new Exception("Convert PPT ke PDF timeout (lebih dari 2 menit).");
                    }

                    if (process.ExitCode != 0)
                        throw new Exception($"soffice keluar dengan kode {process.ExitCode}. Output: {stdOut} | Error: {stdErr}");
                }

                if (!File.Exists(expectedOutputPath))
                    throw new Exception($"Convert selesai tapi file hasil tidak ditemukan di '{expectedOutputPath}'.");

                string finalOutDir = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(finalOutDir))
                    Directory.CreateDirectory(finalOutDir);

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                File.Move(expectedOutputPath, outputPath);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempOutDir)) Directory.Delete(tempOutDir, true);
                    if (Directory.Exists(userProfileDir)) Directory.Delete(userProfileDir, true);
                }
                catch
                {
                    // folder temp gagal dihapus bukan hal fatal, biarkan saja
                }
            }
        }

        public string DecodeFileName(string input)
        {
            if (input.StartsWith("=?utf-8?B?"))
            {
                var base64 = Regex.Match(input, @"\=\?utf-8\?B\?(.*?)\?\=").Groups[1].Value;
                var bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }

            return input;
        }

        [HttpPost]
        [Route("api/document/upload")]
        public HttpResponseMessage Upload()
        {
            var httpRequest = System.Web.HttpContext.Current.Request;

            if (httpRequest.Files.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "File tidak ditemukan");
            }

            var file = httpRequest.Files[0];
            string rawFileName = file.FileName;
            string decodedFileName = DecodeFileName(rawFileName);

            var title = httpRequest["title"];
            var menuId = int.Parse(httpRequest["menuId"]);
            var categoryId = int.Parse(httpRequest["categoryId"]);

            if (string.IsNullOrEmpty(title))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Title wajib diisi");
            }

            if (file.ContentLength > 50 * 1024 * 1024)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "File terlalu besar");
            }

            string ext = Path.GetExtension(decodedFileName).ToLower();

            // ======================
            // SIMPAN FILE UTAMA (D:\SOP\Document\{Tipe}\...)
            // supaya tidak ikut hilang saat project di-publish ulang,
            // konsisten dengan modul Machine.
            // ======================
            var saveInfo = FileStorageHelper.BuildSavePath("Document", decodedFileName);
            file.SaveAs(saveInfo.PhysicalPath);

            string fileName = Path.GetFileName(saveInfo.PhysicalPath);
            string dbPath = saveInfo.RelativePath; // contoh: "Others/nama-file.pptx"

            string previewPath = null;

            try
            {
                if (ext == ".ppt" || ext == ".pptx")
                {
                    string pdfFileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";

                    // preview PDF juga disimpan lewat FileStorageHelper (extension .pdf
                    // otomatis masuk folder "PDF" yang sama dengan upload PDF biasa)
                    var previewSaveInfo = FileStorageHelper.BuildSavePath("Document", pdfFileName);

                    ConvertPptToPdf(saveInfo.PhysicalPath, previewSaveInfo.PhysicalPath);

                    if (File.Exists(previewSaveInfo.PhysicalPath))
                    {
                        previewPath = previewSaveInfo.RelativePath;
                    }
                }
            }
            catch (Exception ex)
            {
                LogConversionError(ex, saveInfo.PhysicalPath);
                previewPath = null;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"INSERT INTO SOP_Documents  
                (Title, FileName, FilePath, FileType, MenuId, CategoryId, PreviewPath, CreatedBy, CreatedDate)
                VALUES (@Title, @FileName, @FilePath, @FileType, @MenuId, @CategoryId, @PreviewPath, @CreatedBy, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@FilePath", dbPath);
                cmd.Parameters.AddWithValue("@FileType", ext);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.Parameters.AddWithValue("@CreatedBy", "admin");
                cmd.Parameters.AddWithValue("@PreviewPath", (object)previewPath ?? DBNull.Value);

                cmd.ExecuteNonQuery();

            }

            return Request.CreateResponse(HttpStatusCode.OK, "Upload success");
        }

        private void LogConversionError(Exception ex, string sourceFile)
        {
            try
            {
                string logFolder = @"D:\SOP\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                string logPath = Path.Combine(logFolder, "document-convert-errors.log");
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Gagal convert '{sourceFile}':\r\n{ex}\r\n\r\n";

                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // logging gagal pun jangan sampai bikin request utama ikut gagal
            }
        }

        /// <summary>
        /// Download file dokumen berdasarkan Id. Menggantikan akses langsung ke
        /// "/Uploads/..." yang sudah tidak berlaku karena file fisik sekarang
        /// disimpan di luar folder aplikasi (D:\SOP\Document\...).
        /// </summary>
        [HttpGet]
        [Route("api/document/download/{id}")]
        public HttpResponseMessage DownloadFile(int id, string username = "")
        {
            if (!AccessHelper.HasDownloadAccess(username))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
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
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return response;
        }

        /// <summary>
        /// Stream file untuk ditampilkan inline di browser (di dalam &lt;iframe&gt;/viewer).
        /// - Kalau ada PreviewPath (hasil convert PPT/PPTX ke PDF), tampilkan itu.
        /// - Kalau tidak ada tapi file aslinya sendiri memang bisa ditampilkan langsung
        ///   di browser (PDF/gambar), tampilkan file aslinya secara inline.
        /// - Kalau tipe file tidak bisa di-preview (docx, xlsx, dll), return NotFound
        ///   supaya sisi UI fallback ke link download biasa.
        /// </summary>
        [HttpGet]
        [Route("api/document/preview/{id}")]
        public HttpResponseMessage PreviewFile(int id, string username = "")
        {
            if (string.IsNullOrEmpty(username))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            string fileName = null;
            string filePath = null;
            string fileType = null;
            string previewPath = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                var cmd = new SqlCommand(
                    "SELECT FileName, FilePath, FileType, PreviewPath FROM SOP_Documents WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var rd = cmd.ExecuteReader();

                if (!rd.Read())
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                fileName = rd["FileName"].ToString();
                filePath = rd["FilePath"].ToString();
                fileType = rd["FileType"].ToString();
                previewPath = rd["PreviewPath"] == DBNull.Value ? null : rd["PreviewPath"].ToString();
            }

            string relativePath;
            string contentType;
            string downloadFileName;

            if (!string.IsNullOrEmpty(previewPath))
            {
                // hasil convert PPT/PPTX -> PDF
                relativePath = previewPath;
                contentType = "application/pdf";
                downloadFileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";
            }
            else if (IsInlineViewable(fileType))
            {
                // file aslinya sendiri sudah bisa ditampilkan langsung (PDF/gambar)
                relativePath = filePath;
                contentType = System.Web.MimeMapping.GetMimeMapping(fileName);
                downloadFileName = fileName;
            }
            else
            {
                // tipe file tidak bisa di-preview di browser (docx, xlsx, dll)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            string fullPath = FileStorageHelper.GetPhysicalPath(relativePath);

            if (fullPath == null || !File.Exists(fullPath))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // "inline" (bukan "attachment") supaya browser menampilkan langsung
            // di dalam iframe, bukan memaksa download.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
            {
                FileName = downloadFileName
            };

            return response;
        }

        private static readonly string[] InlineViewableExtensions =
            { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

        private bool IsInlineViewable(string fileType)
        {
            if (string.IsNullOrEmpty(fileType))
                return false;

            return InlineViewableExtensions.Contains(fileType.ToLower());
        }

        [HttpGet]
        [Route("api/document")]
        public IHttpActionResult GetDocuments(int menuId, int categoryId = 0)
        {
            List<object> docs = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"SELECT * FROM SOP_Documents 
                         WHERE MenuId = @MenuId";

                if (categoryId > 0)
                {
                    query += " AND CategoryId = @CategoryId";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MenuId", menuId);

                if (categoryId > 0)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    docs.Add(new
                    {
                        id = (int)reader["Id"],
                        title = reader["Title"].ToString(),
                        filePath = reader["FilePath"].ToString(),
                        previewPath = reader["PreviewPath"] == DBNull.Value ? null : reader["PreviewPath"].ToString(),
                        fileType = reader["FileType"].ToString()
                    });
                }
            }

            return Ok(docs);
        }

        [HttpGet]
        [Route("api/category")]
        public IHttpActionResult GetCategoryByMenu(int menuId)
        {
            List<object> categories = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT Id, Name FROM SOP_Categories WHERE MenuId = @MenuId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MenuId", menuId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(new
                    {
                        id = (int)reader["Id"],
                        name = reader["Name"].ToString()
                    });
                }
            }

            return Ok(categories);
        }

        [HttpDelete]
        [Route("api/document/{id}")]
        public IHttpActionResult Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // ambil file path & preview path dulu
                string filePath = "";
                string previewPath = "";

                SqlCommand get = new SqlCommand("SELECT FilePath, PreviewPath FROM SOP_Documents WHERE Id=@Id", conn);
                get.Parameters.AddWithValue("@Id", id);

                var reader = get.ExecuteReader();
                if (reader.Read())
                {
                    filePath = reader["FilePath"] == DBNull.Value ? "" : reader["FilePath"].ToString();
                    previewPath = reader["PreviewPath"] == DBNull.Value ? "" : reader["PreviewPath"].ToString();
                }
                reader.Close();

                // delete db
                SqlCommand del = new SqlCommand("DELETE FROM SOP_Documents WHERE Id=@Id", conn);
                del.Parameters.AddWithValue("@Id", id);
                del.ExecuteNonQuery();

                // delete file fisik utama
                if (!string.IsNullOrEmpty(filePath))
                {
                    string fullPath = FileStorageHelper.GetPhysicalPath(filePath);
                    if (fullPath != null && File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                // delete file preview (kalau ada)
                if (!string.IsNullOrEmpty(previewPath))
                {
                    string fullPreviewPath = FileStorageHelper.GetPhysicalPath(previewPath);
                    if (fullPreviewPath != null && File.Exists(fullPreviewPath))
                        File.Delete(fullPreviewPath);
                }
            }

            return Ok();
        }

        [HttpPut]
        [Route("api/document/{id}")]
        public IHttpActionResult Update(int id)
        {
            var httpRequest = System.Web.HttpContext.Current.Request;

            string title = httpRequest["title"];

            if (string.IsNullOrEmpty(title))
                return BadRequest("Title wajib diisi");

            int menuId, categoryId;
            if (!int.TryParse(httpRequest["menuId"], out menuId) ||
                !int.TryParse(httpRequest["categoryId"], out categoryId))
                return BadRequest("menuId/categoryId tidak valid");

            // ambil FilePath & PreviewPath lama dulu — dipakai untuk hapus file
            // fisik lama nanti KALAU ada file baru yang diupload, sekaligus
            // memastikan Id-nya memang ada.
            string oldFilePath = null;
            string oldPreviewPath = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                var getCmd = new SqlCommand("SELECT FilePath, PreviewPath FROM SOP_Documents WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);

                var rd = getCmd.ExecuteReader();
                if (!rd.Read())
                {
                    rd.Close();
                    return NotFound();
                }

                oldFilePath = rd["FilePath"] == DBNull.Value ? null : rd["FilePath"].ToString();
                oldPreviewPath = rd["PreviewPath"] == DBNull.Value ? null : rd["PreviewPath"].ToString();
                rd.Close();
            }

            bool hasNewFile = httpRequest.Files.Count > 0;

            string newFileName = null;
            string newFilePath = null;
            string newFileType = null;
            string newPreviewPath = null;

            if (hasNewFile)
            {
                var file = httpRequest.Files[0];
                string decodedFileName = DecodeFileName(file.FileName);

                if (file.ContentLength > 50 * 1024 * 1024)
                    return BadRequest("File terlalu besar");

                string ext = Path.GetExtension(decodedFileName).ToLower();

                var saveInfo = FileStorageHelper.BuildSavePath("Document", decodedFileName);
                file.SaveAs(saveInfo.PhysicalPath);

                newFileName = Path.GetFileName(saveInfo.PhysicalPath);
                newFilePath = saveInfo.RelativePath;
                newFileType = ext;

                try
                {
                    if (ext == ".ppt" || ext == ".pptx")
                    {
                        string pdfFileName = Path.GetFileNameWithoutExtension(newFileName) + ".pdf";
                        var previewSaveInfo = FileStorageHelper.BuildSavePath("Document", pdfFileName);

                        ConvertPptToPdf(saveInfo.PhysicalPath, previewSaveInfo.PhysicalPath);

                        if (File.Exists(previewSaveInfo.PhysicalPath))
                            newPreviewPath = previewSaveInfo.RelativePath;
                    }
                }
                catch (Exception)
                {
                    // optional: log error
                    newPreviewPath = null;
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd;

                if (hasNewFile)
                {
                    cmd = new SqlCommand(@"
                        UPDATE SOP_Documents
                        SET Title=@Title, MenuId=@MenuId, CategoryId=@CategoryId,
                            FileName=@FileName, FilePath=@FilePath, FileType=@FileType, PreviewPath=@PreviewPath
                        WHERE Id=@Id", conn);

                    cmd.Parameters.AddWithValue("@FileName", newFileName);
                    cmd.Parameters.AddWithValue("@FilePath", newFilePath);
                    cmd.Parameters.AddWithValue("@FileType", newFileType);
                    cmd.Parameters.AddWithValue("@PreviewPath", (object)newPreviewPath ?? DBNull.Value);
                }
                else
                {
                    cmd = new SqlCommand(@"
                        UPDATE SOP_Documents
                        SET Title=@Title, MenuId=@MenuId, CategoryId=@CategoryId
                        WHERE Id=@Id", conn);
                }

                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@MenuId", menuId);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            // hapus file fisik lama SETELAH update DB berhasil — supaya kalau ada
            // kegagalan di tengah jalan, dokumen lama tidak ikut hilang percuma.
            if (hasNewFile)
            {
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    string oldFullPath = FileStorageHelper.GetPhysicalPath(oldFilePath);
                    if (oldFullPath != null && File.Exists(oldFullPath))
                        File.Delete(oldFullPath);
                }

                if (!string.IsNullOrEmpty(oldPreviewPath))
                {
                    string oldPreviewFullPath = FileStorageHelper.GetPhysicalPath(oldPreviewPath);
                    if (oldPreviewFullPath != null && File.Exists(oldPreviewFullPath))
                        File.Delete(oldPreviewFullPath);
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/document/{id}")]
        public IHttpActionResult GetById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM SOP_Documents WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return Ok(new
                    {
                        Id = dr["Id"],
                        Title = dr["Title"],
                        MenuId = dr["MenuId"],
                        CategoryId = dr["CategoryId"]
                    });
                }
            }

            return NotFound();
        }

        [HttpGet]
        [Route("api/document/all")]
        public IHttpActionResult GetAllDocuments()
        {
            List<object> docs = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                        SELECT 
                        A.Id,
                        A.Title,
                        A.FileName,
                        A.FileType,
                        A.FilePath,
                        A.PreviewPath,
                        A.MenuId,
                        A.CategoryId,
                        B.Name AS MenuName,
                        C.Name AS CategoryName
                        FROM SOP_Documents A
                        LEFT JOIN SOP_Menus B ON B.Id = A.MenuId
                        left join SOP_Menus ME ON ME.Id = B.ParentId
                        LEFT JOIN SOP_Categories C ON C.Id = A.CategoryId
                        WHERE ME.Name = 'TECHNICAL LIBRARY DATA'";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    docs.Add(new
                    {
                        id = (int)reader["Id"],
                        title = reader["Title"].ToString(),
                        fileType = reader["FileType"].ToString(),
                        filePath = reader["FilePath"].ToString(),
                        previewPath = reader["PreviewPath"] == DBNull.Value ? null : reader["PreviewPath"].ToString(),

                        menuId = reader["MenuId"],
                        menuName = reader["MenuName"]?.ToString(),

                        categoryId = reader["CategoryId"],
                        categoryName = reader["CategoryName"]?.ToString()
                    });
                }
            }

            return Ok(docs);
        }

        [HttpGet]
        [Route("api/category")]
        public IHttpActionResult GetAll()
        {
            List<object> list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Id, Name FROM SOP_Categories", conn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new
                    {
                        Id = (int)dr["Id"],
                        Name = dr["Name"].ToString()
                    });
                }
            }

            return Ok(list);
        }
    }
}