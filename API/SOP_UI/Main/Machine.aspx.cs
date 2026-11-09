using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI.Main
{
    public partial class Machine : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "10");
                // simpan ke hidden
                await LoadMachine(categoryId);
                await LoadMasterDownload();
            }
        }

        private async Task LoadMachine(int categoryId)
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
            string username = Session["User"]?.ToString() ?? "";

            bool hasAccess = await CheckDownloadAccess(baseUrl, username);

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{baseUrl}/api/machine?categoryId={categoryId}");
                dynamic data = JsonConvert.DeserializeObject(res);

                StringBuilder html = new StringBuilder();

                foreach (var item in data)
                {
                    html.Append("<div class='item-card'>");

                    html.Append($@"
                        <div class='item-title' onclick='toggleItem(this)'>
                            {item.ItemName} <span class='arrow'>▾</span>
                        </div>
                        <div class='item-content'>
                    ");

                    foreach (var type in item.Types)
                    {
                        html.Append($"<div class='type-box'><b>{type.TypeName}</b>");

                        foreach (var file in type.Files)
                        {
                            string fileName = file.FileName.ToString();
                            string ext = System.IO.Path.GetExtension(fileName).ToLower();

                            string icon = ext == ".pdf" ? "📕" :
                                          ext == ".xlsx" || ext == ".xls" ? "📗" :
                                          ext == ".docx" || ext == ".doc" ? "📘" :
                                          (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif") ? "🖼️" : "📄";

                            string[] previewable = { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
                            bool isPreviewable = previewable.Contains(ext);

                            string downloadUrl = $"{baseUrl}/api/machine/download/{file.Id}?username={HttpUtility.UrlEncode(username)}";
                            string previewUrl = $"{baseUrl}/api/machine/download/{file.Id}?mode=preview&username={HttpUtility.UrlEncode(username)}";

                            string previewBtn = isPreviewable
                                ? $@"<a href='javascript:void(0)' class='btn-preview' onclick=""openPreview('{previewUrl}')"">Preview</a>"
                                : "";

                            string downloadBtn = hasAccess
                                ? $@"<a href='{downloadUrl}' class='btn-download' target='_blank'>Download</a>"
                                : $@"<a href='javascript:void(0)' class='btn-download' onclick=""showNoAccess()"">Download</a>";


                            html.Append($@"
                                <div class='file-row'>
                                    <span class='file-name'>{icon} {fileName}</span>
                                    <div class='file-actions'>
                                        {previewBtn}
                                        {downloadBtn}
                                    </div>
                                </div>
                            ");
                        }

                        html.Append("</div>");
                    }

                    html.Append("</div></div>");
                }

                machineContainer.InnerHtml = html.ToString();
            }
        }

        private async Task<bool> CheckDownloadAccess(string baseUrl, string username)
        {
            if (string.IsNullOrEmpty(username)) return false;

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{baseUrl}/api/access/check?username={HttpUtility.UrlEncode(username)}");
                dynamic result = JsonConvert.DeserializeObject(res);
                return (bool)result.hasAccess;
            }
        }

        protected async Task LoadMasterDownload()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
            string username = Session["User"]?.ToString() ?? "";
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{baseUrl}/api/machine/master/download");

                dynamic data = JsonConvert.DeserializeObject(res);

                downloadLink.HRef = $"{baseUrl}/api/machine/download/{data.Id}?username={HttpUtility.UrlEncode(username)}";
                downloadLink.Attributes["download"] = data.FileName;
            }
        }
    }
}