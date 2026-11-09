using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

namespace SOP_UI.Main
{
    public partial class DocumentPage : System.Web.UI.Page
    {
        private string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];

        public class Doc
        {
            public int id { get; set; }
            public string title { get; set; }
            public string filePath { get; set; }
            public string fileType { get; set; }

            public string previewPath { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int menuId = int.Parse(Request.QueryString["menuId"] ?? "0");
            int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "0");

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    string menuName = await GetMenuName(menuId);
                    lblTitle.Text = "📄 " + menuName;
                    var children = await GetChildMenus(menuId);

                    if (children.Count > 0)
                    {
                        // MASIH ADA SUBMENU → tampilkan menu tab
                        await RenderMenuTabs(menuId);

                        // kosongkan document
                        rptDocs.DataSource = null;
                        rptDocs.DataBind();
                    }
                    else
                    {
                        // LEVEL TERAKHIR → tampilkan category + document
                        await RenderCategoryTabs(menuId, categoryId);
                        await LoadDocuments(menuId, categoryId);
                    }
                }));
            }
        }
        private bool _hasAccess = false;

        private async Task LoadDocuments(int menuId, int categoryId)
        {
            string baseUrl = ApiBaseUrl;
            string username = Session["User"]?.ToString() ?? "";

            _hasAccess = await CheckDownloadAccess(baseUrl, username);
            hfHasAccess.Value = _hasAccess ? "true" : "false";

            string encodedUser = HttpUtility.UrlEncode(username);

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(
                    $"{baseUrl}/api/document?menuId={menuId}&categoryId={categoryId}"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<Doc>>(json);

                    string[] inlineViewableTypes = { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

                    foreach (var d in data)
                    {
                        d.filePath = $"{baseUrl}/api/document/download/{d.id}?username={encodedUser}";

                        bool hadPreview = !string.IsNullOrEmpty(d.previewPath);
                        bool isInlineViewable = inlineViewableTypes.Contains((d.fileType ?? "").ToLower());

                        d.previewPath = (hadPreview || isInlineViewable)
                            ? $"{baseUrl}/api/document/preview/{d.id}?username={encodedUser}"
                            : null;
                    }

                    rptDocs.DataSource = data;
                    rptDocs.DataBind();
                }
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

        protected string GetThumbHtml(object previewPath, object fileType)
        {
            bool hasPreview = previewPath != null
                && previewPath != DBNull.Value
                && !string.IsNullOrEmpty(previewPath.ToString());

            if (hasPreview)
            {
                string url = previewPath.ToString();
                return $@"
            <iframe src='{url}#toolbar=0&zoom=page-width'></iframe>
            <div class='arrow-btn' onclick=""openPreview('{url}')"">↗</div>
        ";
            }
            else
            {
                string iconUrl = GetFileIcon(fileType);
                return $@"
            <div class='doc-icon-fallback'>
                <img src='{iconUrl}' alt='file icon' />
            </div>
        ";
            }
        }

        public string GetDownloadOnClick(object filePath)
        {
            string url = filePath?.ToString() ?? "#";
            return $"handleDownload(\"{url}\")";
        }

        public class Category
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private async Task<List<Category>> GetCategories(int menuId)
        {
            using (var client = new HttpClient())
            {
                var res = await client.GetStringAsync(
                    $"{ApiBaseUrl}/api/category?menuId={menuId}"
                );

                return JsonConvert.DeserializeObject<List<Category>>(res);
            }
        }

        private async Task RenderCategoryTabs(int menuId, int categoryId)
        {
            var categories = await GetCategories(menuId);

            string html = "<div class='tabs'>";

            // ALL
            string activeClass = categoryId == 0 ? "active-tab" : "";
            html += $"<a href='?menuId={menuId}&categoryId=0' class='tab {activeClass}'>All</a>";

            foreach (var c in categories)
            {
                activeClass = c.id == categoryId ? "active-tab" : "";

                html += $"<a href='?menuId={menuId}&categoryId={c.id}' class='tab {activeClass}'>{c.name}</a>";
            }

            html += "</div>";

            tabContainer.InnerHtml = html;
        }

        private async Task<List<MenuDTO>> GetChildMenus(int parentId)
        {
            using (var client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");
                var allMenus = JsonConvert.DeserializeObject<List<MenuDTO>>(res);

                return allMenus
                    .Where(x => x.ParentId == parentId)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
            }
        }

        private async Task RenderMenuTabs(int menuId)
        {
            var children = await GetChildMenus(menuId);

            string html = "<div class='tabs'>";

            foreach (var c in children)
            {
                html += $"<a href='?menuId={c.Id}' class='tab'>{c.Name}</a>";
            }

            html += "</div>";

            tabContainer.InnerHtml = html;
        }

        protected string GetFileIcon(object fileType)
        {
            var ext = fileType?.ToString().ToLower() ?? "";

            if (ext.Contains("pdf")) return "/assets/pdf.png";
            if (ext.Contains("xls")) return "/assets/excel.png";
            if (ext.Contains("doc")) return "/assets/word.png";
            if (ext.Contains("ppt")) return "/assets/ppt.png";

            return "/assets/file.png";
        }

        private async Task<string> GetMenuName(int menuId)
        {
            using (var client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");
                var menus = JsonConvert.DeserializeObject<List<MenuDTO>>(res);

                var menu = menus.FirstOrDefault(x => x.Id == menuId);

                return menu != null ? menu.Name : "Documents";
            }
        }

        protected bool IsPreviewable(object previewPath)
        {
            return previewPath != null && previewPath != DBNull.Value
                && !string.IsNullOrEmpty(previewPath.ToString());
        }
    }
}