using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SOP_UI.Main
{
    public partial class SOP : System.Web.UI.Page
    {
        private string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];

        protected void Page_Load(object sender, EventArgs e)
        {
            int menuId = int.Parse(Request.QueryString["menuId"] ?? "0");

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadMenuTitle(menuId);
                    await LoadSOP(menuId);
                }));
            }
        }

        private async Task LoadSOP(int menuId)
        {
            string baseUrl = ApiBaseUrl;
            string username = Session["User"]?.ToString() ?? "";

            bool hasAccess = await CheckDownloadAccess(baseUrl, username);

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync(
                    $"{baseUrl}/api/sop?menuId={menuId}"
                );

                dynamic data = JsonConvert.DeserializeObject(res);

                StringBuilder html = new StringBuilder();

                foreach (var section in data)
                {
                    html.Append("<div class='section-card'>");
                    html.Append("<div class='row g-3'>");

                    html.Append("<div class='col-md-4'>");
                    html.Append($@"
                        <img src='{baseUrl}/api/sop/section/image/{section.id}' 
                            class='section-img' 
                            style='cursor:pointer' 
                            onload='checkImageOrientation(this)'
                            onclick='openImageModal(this.src)' />
                ");
                    html.Append("</div>");

                    html.Append("<div class='col-md-8 sop-right'>");

                    html.Append($@"
                        <div class='sop-header-center'>
                            <div class='main-title'>SOP & Work Instruction</div>
                            <div class='sub-title'>{section.name}</div>
                            <div class='underline'></div>
                        </div>
                    ");

                    foreach (var group in section.groups)
                    {
                        html.Append("<div class='group-box' onclick='toggleGroup(this)'>");

                        html.Append($@"
                            <div class='group-header'>
                                {group.name}
                                <span class='arrow'>▾</span>
                            </div>
                        ");

                        html.Append("<div class='group-items'>");

                        foreach (var item in group.items)
                        {
                            string encodedUser = HttpUtility.UrlEncode(username);
                            string fileUrl = $"{baseUrl}/api/sop/item/download/{item.id}?username={encodedUser}";
                            string previewUrl = $"{baseUrl}/api/sop/item/download/{item.id}?mode=preview&username={encodedUser}";

                            string ext = System.IO.Path.GetExtension((string)item.file ?? "")?.ToLower() ?? "";
                            string[] inlineViewableTypes = { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
                            bool isPreviewable = inlineViewableTypes.Contains(ext);

                            string previewBtn = isPreviewable
                                 ? $@"<a href='javascript:void(0)' class='btn-preview' onclick=""openPreview('{previewUrl}')"">Preview</a>"
                                 : "";

                            string downloadBtn = hasAccess
                                ? $@"<a href='{fileUrl}' class='btn-download' target='_blank'>Download</a>"
                                : $@"<a href='javascript:void(0)' class='btn-download' onclick=""showNoAccess()"">Download</a>";

                            html.Append($@"
                            <div class='item-row-modern'>
                                <span>📄 {item.title}</span>
                                <div>
                                    {previewBtn}
                                    {downloadBtn}
                                </div>
                            </div>
                            ");
                        }

                        html.Append("</div>");
                        html.Append("</div>");
                    }

                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("</div>");
                }

                sopContainer.InnerHtml = html.ToString();
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

        private async Task LoadMenuTitle(int menuId)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");

                var menus = JsonConvert.DeserializeObject<List<MenuDTO>>(res);

                var menu = menus.FirstOrDefault(x => x.Id == menuId);

                if (menu != null)
                {
                    lblMenuTitle.Text = menu.Name.ToUpper();
                }
            }
        }
    }
}
