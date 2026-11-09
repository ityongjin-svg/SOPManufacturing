using SOP_API.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace SOP_API.Controllers
{
    [RoutePrefix("api/qclayout")]
    public class QCLayoutController : ApiController
    {
        string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var list = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT Id, Buyer, StyleName, Item, Sort, Description, VideoFileName, VideoPath, FileFileName, FilePath, ThumbnailFileName, ThumbnailPath, Status
                    FROM SOP_QCStyle
                    ORDER BY CreatedDate DESC", conn);

                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    list.Add(new
                    {
                        Id = (int)rd["Id"],
                        Buyer = rd["Buyer"].ToString(),
                        StyleName = rd["StyleName"].ToString(),
                        Item = rd["Item"].ToString(),
                        Sort = rd["Sort"].ToString(),
                        Description = rd["Description"] == DBNull.Value ? "" : rd["Description"].ToString(),
                        HasVideo = rd["VideoPath"] != DBNull.Value,
                        HasThumbnail = rd["ThumbnailPath"] != DBNull.Value,
                        HasFile = rd["FilePath"] != DBNull.Value,
                        FileFileName = rd["FileFileName"] == DBNull.Value ? "" : rd["FileFileName"].ToString(),
                        Status = rd["Status"].ToString()
                    });
                }
            }

            return Ok(list);
        }

        [HttpGet]
        [Route("buyers")]
        public IHttpActionResult GetBuyers()
        {
            var list = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT DISTINCT [NAME] FROM [SOP_BUYER] WHERE IS_ACTIVE = 1 order by[NAME] ASC", conn);
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                    list.Add(rd["NAME"].ToString());
            }
            return Ok(list);
        }

        [HttpGet]
        [Route("items")]
        public IHttpActionResult GetItemOptions()
        {
            var list = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT KETERANGAN FROM SOP_SORT_ITEM WHERE CATEGORY = 'Item' ORDER BY KETERANGAN ASC", conn);
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                    list.Add(rd["KETERANGAN"].ToString());
            }
            return Ok(list);
        }

        [HttpGet]
        [Route("sorts")]
        public IHttpActionResult GetSortOptions()
        {
            var list = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT KETERANGAN FROM SOP_SORT_ITEM WHERE CATEGORY = 'Sort' ORDER BY KETERANGAN ASC", conn);
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                    list.Add(rd["KETERANGAN"].ToString());
            }
            return Ok(list);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM SOP_QCStyle WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    return NotFound();

                return Ok(new
                {
                    Id = rd["Id"],
                    Buyer = rd["Buyer"].ToString(),
                    StyleName = rd["StyleName"].ToString(),
                    Item = rd["Item"].ToString(),
                    Sort = rd["Sort"].ToString(),
                    Description = rd["Description"] == DBNull.Value ? "" : rd["Description"].ToString(),
                    VideoFileName = rd["VideoFileName"] == DBNull.Value ? "" : rd["VideoFileName"].ToString(),
                    FileFileName = rd["FileFileName"] == DBNull.Value ? "" : rd["FileFileName"].ToString()
                });
            }
        }

        [HttpPost]
        [Route("upload")]
        public IHttpActionResult Upload()
        {
            var request = HttpContext.Current.Request;

            string Buyer = request["Buyer"];
            string styleName = request["StyleName"];
            string Item= request["Item"];
            string Sort = request["Sort"];
            string description = request["Description"];
            string createdBy = request["CreatedBy"] ?? "admin";

            if (string.IsNullOrWhiteSpace(Buyer))
                return BadRequest("Buyer Name wajib diisi");

            if (string.IsNullOrWhiteSpace(styleName))
                return BadRequest("Style Name wajib diisi");

            if (string.IsNullOrWhiteSpace(Item))
                return BadRequest("Item Name wajib diisi");

            if (string.IsNullOrWhiteSpace(Sort))
                return BadRequest("Sort Name wajib diisi");

            string fileFileName = null, fileRelPath = null;
            if (request.Files["file"] != null && request.Files["file"].ContentLength > 0)
            {
                var file = request.Files["file"];
                var saveInfo = FileStorageHelper.BuildSavePath("QCLayout", file.FileName);
                file.SaveAs(saveInfo.PhysicalPath);
                fileFileName = Path.GetFileName(saveInfo.PhysicalPath);
                fileRelPath = saveInfo.RelativePath;
            }

            bool hasVideo = request.Files["video"] != null && request.Files["video"].ContentLength > 0;
            string tempVideoPath = null;
            string originalVideoName = null;

            if (hasVideo)
            {
                var video = request.Files["video"];
                originalVideoName = video.FileName;
                tempVideoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(video.FileName));
                video.SaveAs(tempVideoPath);
            }

            int newId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                INSERT INTO SOP_QCStyle
                (Buyer, StyleName, Item, Sort , Description, FileFileName, FilePath, CreatedBy, CreatedDate, Status)
                OUTPUT INSERTED.Id
                VALUES (@Buyer, @StyleName, @Item, @Sort, @Description, @FileFileName, @FilePath, @CreatedBy, GETDATE(), @Status)", conn);

                cmd.Parameters.AddWithValue("@Buyer", Buyer);
                cmd.Parameters.AddWithValue("@StyleName", styleName);
                cmd.Parameters.AddWithValue("@Item", Item);
                cmd.Parameters.AddWithValue("@Sort", Sort);
                cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FileFileName", (object)fileFileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FilePath", (object)fileRelPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                cmd.Parameters.AddWithValue("@Status", hasVideo ? "Processing" : "Ready");

                newId = (int)cmd.ExecuteScalar();
            }

            if (hasVideo)
            {
                int capturedId = newId;
                string capturedTempPath = tempVideoPath;
                string capturedOriginalName = originalVideoName;

                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                    ProcessVideoInBackground(capturedId, capturedTempPath, capturedOriginalName));
            }

            return Ok(new { Id = newId });
        }

        [HttpPost]
        [Route("update/{id}")]
        public IHttpActionResult Update(int id)
        {
            var request = HttpContext.Current.Request;

            string Buyer = request["Buyer"];
            string styleName = request["StyleName"];
            string Item = request["Item"];
            string Sort = request["Sort"];
            string description = request["Description"];
            bool hasNewVideo = request.Files["video"] != null && request.Files["video"].ContentLength > 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string oldFilePath = null, oldFileFileName = null;

                var getCmd = new SqlCommand("SELECT FilePath, FileFileName FROM SOP_QCStyle WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);

                using (var rd = getCmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        oldFilePath = rd["FilePath"] == DBNull.Value ? null : rd["FilePath"].ToString();
                        oldFileFileName = rd["FileFileName"] == DBNull.Value ? null : rd["FileFileName"].ToString();
                    }
                }

                string fileFileName = oldFileFileName, fileRelPath = oldFilePath;

                if (request.Files["file"] != null && request.Files["file"].ContentLength > 0)
                {
                    var file = request.Files["file"];
                    var saveInfo = FileStorageHelper.BuildSavePath("QCLayout", file.FileName);
                    file.SaveAs(saveInfo.PhysicalPath);
                    fileFileName = Path.GetFileName(saveInfo.PhysicalPath);
                    fileRelPath = saveInfo.RelativePath;
                }

                string statusClause = hasNewVideo ? ", Status='Processing'" : "";

                var cmd = new SqlCommand($@"
                UPDATE SOP_QCStyle
                SET 
                    Buyer=@Buyer, 
                    StyleName=@StyleName, 
                    Item=@Item, 
                    Sort=@Sort, 
                    Description=@Description,
                    FileFileName=@FileFileName, 
                    FilePath=@FilePath,
                    UpdatedDate=GETDATE()
                    {statusClause}
                WHERE Id=@Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Buyer", Buyer);
                cmd.Parameters.AddWithValue("@StyleName", styleName);
                cmd.Parameters.AddWithValue("@Item", Item);
                cmd.Parameters.AddWithValue("@Sort", Sort);
                cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FileFileName", (object)fileFileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FilePath", (object)fileRelPath ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                if (hasNewVideo)
                {
                    var video = request.Files["video"];
                    string tempVideoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(video.FileName));
                    video.SaveAs(tempVideoPath);

                    string originalName = video.FileName;
                    int capturedId = id;

                    HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                        ProcessVideoInBackground(capturedId, tempVideoPath, originalName));
                }
            }

            return Ok("Updated");
        }

        private void ProcessVideoInBackground(int id, string tempVideoPath, string originalFileName)
        {
            string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            try
            {
                var saveInfo = FileStorageHelper.BuildSavePath("QCLayout", Path.GetFileNameWithoutExtension(originalFileName) + ".mp4");
                VideoHelper.CompressVideo(tempVideoPath, saveInfo.PhysicalPath);

                string videoFileName = Path.GetFileName(saveInfo.PhysicalPath);
                string videoRelPath = saveInfo.RelativePath;

                var thumbSaveInfo = FileStorageHelper.BuildSavePath("QCLayout", Path.GetFileNameWithoutExtension(videoFileName) + ".jpg");
                VideoHelper.GenerateThumbnail(saveInfo.PhysicalPath, thumbSaveInfo.PhysicalPath);

                string thumbFileName = Path.GetFileName(thumbSaveInfo.PhysicalPath);
                string thumbRelPath = thumbSaveInfo.RelativePath;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                    UPDATE SOP_QCStyle
                    SET VideoFileName=@VideoFileName, VideoPath=@VideoPath,
                        ThumbnailFileName=@ThumbnailFileName, ThumbnailPath=@ThumbnailPath,
                        Status='Ready'
                    WHERE Id=@Id", conn);

                    cmd.Parameters.AddWithValue("@VideoFileName", videoFileName);
                    cmd.Parameters.AddWithValue("@VideoPath", videoRelPath);
                    cmd.Parameters.AddWithValue("@ThumbnailFileName", thumbFileName);
                    cmd.Parameters.AddWithValue("@ThumbnailPath", thumbRelPath);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("UPDATE SOP_QCStyle SET Status='Failed' WHERE Id=@Id", conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                System.Diagnostics.Debug.WriteLine("QC Video processing failed: " + ex.Message);
            }
            finally
            {
                if (File.Exists(tempVideoPath))
                    File.Delete(tempVideoPath);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string videoPath = null, filePath = null;

                var getCmd = new SqlCommand("SELECT VideoPath, FilePath FROM SOP_QCStyle WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);

                using (var rd = getCmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        videoPath = rd["VideoPath"] == DBNull.Value ? null : rd["VideoPath"].ToString();
                        filePath = rd["FilePath"] == DBNull.Value ? null : rd["FilePath"].ToString();
                    }
                }

                var del = new SqlCommand("DELETE FROM SOP_QCStyle WHERE Id=@Id", conn);
                del.Parameters.AddWithValue("@Id", id);
                del.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(videoPath))
                {
                    string full = FileStorageHelper.GetPhysicalPath(videoPath);
                    if (full != null && File.Exists(full)) File.Delete(full);
                }
                if (!string.IsNullOrEmpty(filePath))
                {
                    string full = FileStorageHelper.GetPhysicalPath(filePath);
                    if (full != null && File.Exists(full)) File.Delete(full);
                }
            }

            return Ok();
        }

        // VIDEO: cukup sudah login (sama seperti Preview dokumen)
        [HttpGet]
        [Route("video/{id}")]
        public HttpResponseMessage StreamVideo(int id, string username = "", string mode = "stream")
        {
            if (mode == "download")
            {
                if (!AccessHelper.HasDownloadAccess(username))
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            else
            {
                if (string.IsNullOrEmpty(username))
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            string fileName = null, relativePath = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT VideoFileName, VideoPath FROM SOP_QCStyle WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                fileName = rd["VideoFileName"] == DBNull.Value ? null : rd["VideoFileName"].ToString();
                relativePath = rd["VideoPath"] == DBNull.Value ? null : rd["VideoPath"].ToString();
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

            if (mode == "download")
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName ?? Path.GetFileName(fullPath)
                };
            }
            return response;
        }

        // FILE: wajib DOWNLOAD_ACCESS = 1 (sama seperti Download dokumen)
        [HttpGet]
        [Route("file/{id}")]
        public HttpResponseMessage DownloadFile(int id, string username = "")
        {
            //if (!AccessHelper.HasDownloadAccess(username))
            //    return new HttpResponseMessage(HttpStatusCode.Forbidden);

            string fileName = null, relativePath = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT FileFileName, FilePath FROM SOP_QCStyle WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                fileName = rd["FileFileName"].ToString();
                relativePath = rd["FilePath"].ToString();
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
                System.Web.MimeMapping.GetMimeMapping(fileName));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return response;
        }

        [HttpGet]
        [Route("thumbnail/{id}")]
        public HttpResponseMessage GetThumbnail(int id)
        {
            string relativePath = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ThumbnailPath FROM SOP_QCStyle WHERE Id=@Id", conn);
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
    }
}