using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SOP_UI.Main
{
    public partial class QCLayout : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                await LoadStyles();
        }

        private async Task LoadStyles()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
            string username = Session["User"]?.ToString() ?? "";
            string encodedUser = HttpUtility.UrlEncode(username);

            bool hasFileAccess = await CheckDownloadAccess(baseUrl, username);
            hfHasFileAccess.Value = hasFileAccess ? "true" : "false";

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{baseUrl}/api/qclayout");
                var data = JsonConvert.DeserializeObject<List<dynamic>>(res);

                StringBuilder html = new StringBuilder();
                int index = 0;

                if (data.Count == 0)
                {
                    styleContainer.InnerHtml = @"
                    <div class='col-12 text-center py-5'>
                        <div style='font-size:48px;'>🗂️</div>
                        <div style='color:#64748b; margin-top:10px;'>Belum ada data QC Layout</div>
                    </div>";
                    return;
                }

                var buyerSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var styleSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var itemSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var sortSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var style in data)
                {
                    int id = (int)style.Id;
                    string buyer = (string)style.Buyer;
                    string name = (string)style.StyleName;
                    string item = (string)style.Item;
                    string sort = (string)style.Sort;
                    string desc = (string)style.Description;

                    buyerSet.Add(buyer);
                    styleSet.Add(name);
                    itemSet.Add(item);
                    sortSet.Add(sort);

                    bool hasVideo = (bool)style.HasVideo;
                    bool hasThumb = (bool)style.HasThumbnail;
                    bool hasFile = (bool)style.HasFile;
                    string status = (string)style.Status;

                    string videoUrl = $"{baseUrl}/api/qclayout/video/{id}?username={encodedUser}";
                    string videoDownloadUrl = $"{baseUrl}/api/qclayout/video/{id}?mode=download&username={encodedUser}";
                    string fileUrl = $"{baseUrl}/api/qclayout/file/{id}";
                    string thumbUrl = $"{baseUrl}/api/qclayout/thumbnail/{id}";

                    string thumbHtml;

                    if (status == "Processing")
                        thumbHtml = @"<div class='style-thumb-fallback'>⏳<br/><span style='font-size:12px;'>Sedang diproses...</span></div>";
                    else if (status == "Failed")
                        thumbHtml = @"<div class='style-thumb-fallback'>⚠️<br/><span style='font-size:12px;'>Video gagal diproses</span></div>";
                    else if (hasThumb)
                        thumbHtml = $@"<img src='{thumbUrl}' class='style-thumb' onclick=""openVideo('{videoUrl}')"" />
                       <div class='play-overlay' onclick=""openVideo('{videoUrl}')"">▶</div>";
                    else
                        thumbHtml = @"<div class='style-thumb-fallback'>🎬</div>";

                    double delay = index * 0.08; // 80ms antar card
                    html.Append($"<div class='col-lg-4 col-md-6 mb-4'>");
                    html.Append($@"<div class='style-card' data-buyer='{HttpUtility.HtmlAttributeEncode(buyer)}' data-style='{HttpUtility.HtmlAttributeEncode(name)}' data-item='{HttpUtility.HtmlAttributeEncode(item)}' data-sort='{HttpUtility.HtmlAttributeEncode(sort)}' style='animation-delay:{delay.ToString(System.Globalization.CultureInfo.InvariantCulture)}s'>");
                    
                    string badges = "<div class='style-badges'>";
                    if (hasVideo && status == "Ready") badges += "<span class='style-badge'>🎥 Video</span>";
                    if (hasFile) badges += "<span class='style-badge'>📄 File</span>";
                    badges += "</div>";

                    html.Append("<div class='style-thumb-wrap'>");
                    html.Append(badges);
                    html.Append(thumbHtml);
                    html.Append("</div>");
                    html.Append("<div class='style-body'>");
                    html.Append($"<div class='style-title'>{buyer}</div>");
                    html.Append($"<div class='style-meta'>{name} &middot; {item} &middot; {sort}</div>");
                    html.Append($"<div class='style-desc'>{desc}</div>");

                    html.Append("<div class='style-actions'>");

                    if (hasVideo && status == "Ready")
                    {
                        string videoBtn = hasFileAccess
                            ? $@"<a href='{videoDownloadUrl}' class='btn-action btn-video' target='_blank'>🎥 Download Video</a>"
                            : $@"<a href='javascript:void(0)' class='btn-action btn-video' onclick=""showNoAccess()"">🎥 Download Video</a>";

                        html.Append(videoBtn);
                    }

                    //if (hasFile)
                    //{
                    //    string fileBtn = true
                    //        ? $@"<a href='{fileUrl}' class='btn-action btn-file' target='_blank'>📄 Layout Chart</a>"
                    //        : $@"<a href='javascript:void(0)' class='btn-action btn-file' onclick=""showNoAccess()"">📄 Layout Chart</a>";

                    //    html.Append(fileBtn);
                    //}

                    if (hasFile)
                    {
                        html.Append($@"<a href='{fileUrl}' class='btn-action btn-file' target='_blank'>📄 Layout Chart</a>");
                    }

                    html.Append("</div>"); // close style-actions
                    html.Append("</div></div></div>");
                }

                filterBar.InnerHtml = BuildFilterOptions(buyerSet, styleSet, itemSet, sortSet);
                styleContainer.InnerHtml = html.ToString();
            }
        }

        private string BuildFilterOptions(IEnumerable<string> buyers, IEnumerable<string> styles, IEnumerable<string> items, IEnumerable<string> sorts)
        {
            var sb = new StringBuilder();

            sb.Append(BuildFilterInput("filterBuyer", "buyerOptions", "Cari/pilih Buyer...", buyers));
            sb.Append(BuildFilterInput("filterStyle", "styleOptions", "Cari/pilih Style...", styles));
            sb.Append(BuildFilterInput("filterItem", "itemOptions", "Cari/pilih Item...", items));
            sb.Append(BuildFilterInput("filterSort", "sortOptions", "Cari/pilih Sort...", sorts));

            return sb.ToString();
        }

        private string BuildFilterInput(string inputId, string listId, string placeholder, IEnumerable<string> options)
        {
            var sb = new StringBuilder();

            sb.Append($@"<input type='text' id='{inputId}' list='{listId}' class='qc-filter-select' placeholder='{placeholder}' autocomplete='off' oninput='searchStyle()' />");
            sb.Append($"<datalist id='{listId}'>");
            foreach (var o in options)
                sb.Append($"<option value='{HttpUtility.HtmlAttributeEncode(o)}'>");
            sb.Append("</datalist>");

            return sb.ToString();
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
    }
}