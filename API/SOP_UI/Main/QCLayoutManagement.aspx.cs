using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Text;
using System.Web;

namespace SOP_UI.Main
{
    public partial class QCLayoutManagement : System.Web.UI.Page
    {
        string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"] + "/api/qclayout/";

        private int EditId
        {
            get { return ViewState["EditId"] == null ? 0 : (int)ViewState["EditId"]; }
            set { ViewState["EditId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropdowns();
                LoadList();
            }
        }

        private List<string> FetchList(string endpoint)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + endpoint).Result;
                return JsonConvert.DeserializeObject<List<string>>(res);
            }
        }

        private void LoadDropdowns()
        {
            var buyers = FetchList("buyers");
            var items = FetchList("items");
            var sorts = FetchList("sorts");

            ddlBuyer.Items.Clear();
            ddlBuyer.Items.Add(new ListItem("-- Pilih Buyer --", ""));
            foreach (var b in buyers) ddlBuyer.Items.Add(new ListItem(b, b));

            ddlItem.Items.Clear();
            ddlItem.Items.Add(new ListItem("-- Pilih Item --", ""));
            foreach (var i in items) ddlItem.Items.Add(new ListItem(i, i));

            ddlSort.Items.Clear();
            ddlSort.Items.Add(new ListItem("-- Pilih Sort --", ""));
            foreach (var s in sorts) ddlSort.Items.Add(new ListItem(s, s));

        }

        private void SetDropdownValue(DropDownList ddl, string value)
        {
            var li = ddl.Items.FindByValue(value);
            ddl.ClearSelection();
            if (li != null) li.Selected = true;
        }

        protected void gvStyle_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var rowData = (QCStyleDTO)e.Row.DataItem;
                e.Row.Attributes["data-buyer"] = rowData.Buyer;
                e.Row.Attributes["data-style"] = rowData.StyleName;
                e.Row.Attributes["data-item"] = rowData.Item;
                e.Row.Attributes["data-sort"] = rowData.Sort;
            }
        }

        private void LoadList()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl).Result;
                var data = JsonConvert.DeserializeObject<List<QCStyleDTO>>(res);

                bool hasProcessing = data.Exists(d => d.Status == "Processing");
                hfHasProcessing.Value = hasProcessing ? "true" : "false";

                gvStyle.DataSource = data;
                gvStyle.DataBind();

                var buyers = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var styles = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var items = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                var sorts = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var d in data)
                {
                    buyers.Add(d.Buyer);
                    styles.Add(d.StyleName);
                    items.Add(d.Item);
                    sorts.Add(d.Sort);
                }

                filterBar.InnerHtml = BuildFilterInputs(buyers, styles, items, sorts);
            }
        }

        private string BuildFilterInputs(IEnumerable<string> buyers, IEnumerable<string> styles, IEnumerable<string> items, IEnumerable<string> sorts)
        {
            var sb = new StringBuilder();
            sb.Append(BuildFilterInput("filterBuyer", "mgmtBuyerOptions", "Cari/pilih Buyer...", buyers));
            sb.Append(BuildFilterInput("filterStyle", "mgmtStyleOptions", "Cari/pilih Style...", styles));
            sb.Append(BuildFilterInput("filterItem", "mgmtItemOptions", "Cari/pilih Item...", items));
            sb.Append(BuildFilterInput("filterSort", "mgmtSortOptions", "Cari/pilih Sort...", sorts));
            return sb.ToString();
        }

        private string BuildFilterInput(string inputId, string listId, string placeholder, IEnumerable<string> options)
        {
            var sb = new StringBuilder();
            sb.Append($@"<input type='text' id='{inputId}' list='{listId}' class='form-control' placeholder='{placeholder}' autocomplete='off' oninput='filterGrid()' />");
            sb.Append($"<datalist id='{listId}'>");
            foreach (var o in options)
                sb.Append($"<option value='{HttpUtility.HtmlAttributeEncode(o)}'>");
            sb.Append("</datalist>");
            return sb.ToString();
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5); // cukup untuk upload file besar, bukan untuk nunggu kompres

                var form = new MultipartFormDataContent();

                form.Add(new StringContent(ddlBuyer.SelectedValue), "Buyer");
                form.Add(new StringContent(txtStyleName.Text), "StyleName");
                form.Add(new StringContent(ddlItem.SelectedValue), "Item");
                form.Add(new StringContent(ddlSort.SelectedValue), "Sort");
                form.Add(new StringContent(txtDescription.Text), "Description");
                form.Add(new StringContent(Session["User"]?.ToString() ?? "admin"), "CreatedBy");

                if (fuVideo.HasFile)
                    form.Add(new StreamContent(fuVideo.FileContent), "video", fuVideo.FileName);

                if (fuFile.HasFile)
                    form.Add(new StreamContent(fuFile.FileContent), "file", fuFile.FileName);

                try
                {
                    HttpResponseMessage response = EditId == 0
                        ? await client.PostAsync(baseUrl + "upload", form)
                        : await client.PostAsync(baseUrl + $"update/{EditId}", form);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        lblMsg.Text = $"Gagal menyimpan ({(int)response.StatusCode}): {errorBody}";
                        return;
                    }
                }
                catch (TaskCanceledException)
                {
                    lblMsg.Text = "Upload timeout — file terlalu besar.";
                    return;
                }
            }

            ResetForm();
            LoadList();
        }

        protected void gvStyle_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvStyle.DataKeys[index].Value);

            if (e.CommandName == "editBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    var res = client.GetStringAsync(baseUrl + id).Result;
                    var item = JsonConvert.DeserializeObject<QCStyleDetailDTO>(res);

                    EditId = id;
                    SetDropdownValue(ddlBuyer, item.Buyer);
                    txtStyleName.Text = item.StyleName;
                    SetDropdownValue(ddlItem, item.Item);
                    SetDropdownValue(ddlSort, item.Sort);
                    txtDescription.Text = item.Description;

                    lblCurrentVideo.Text = string.IsNullOrEmpty(item.VideoFileName)
                        ? "(belum ada video)" : $"Video saat ini: {item.VideoFileName}";
                    lblCurrentFile.Text = string.IsNullOrEmpty(item.FileFileName)
                        ? "(belum ada file)" : $"File saat ini: {item.FileFileName}";

                    btnSave.Text = "Update Style";
                }
            }

            if (e.CommandName == "deleteBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + id).Wait();
                }

                LoadList();
            }
        }

        private void ResetForm()
        {
            ddlBuyer.SelectedIndex = 0;
            txtStyleName.Text = "";
            ddlItem.SelectedIndex = 0;
            ddlSort.SelectedIndex = 0;
            txtDescription.Text = "";
            lblCurrentVideo.Text = "";
            lblCurrentFile.Text = "";
            EditId = 0;
            btnSave.Text = "Save Style";
        }
    }
}