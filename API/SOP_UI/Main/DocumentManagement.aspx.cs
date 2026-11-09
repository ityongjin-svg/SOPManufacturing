using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI.Main
{
    public partial class DocumentManagement : System.Web.UI.Page
    {
        private string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadMenus();
                    await LoadCategories();
                    await LoadDocuments();
                }));
            }
        }

        private async Task LoadDocuments()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/document/all");

                var data = JsonConvert.DeserializeObject<List<DocumentDTO>>(res);

                gvDocs.DataSource = data;
                gvDocs.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                if (fileUpload.HasFile)
                {
                    var fileContent = new StreamContent(fileUpload.FileContent);
                    form.Add(fileContent, "file", fileUpload.FileName);
                }

                form.Add(new StringContent(txtTitle.Text), "title");
                form.Add(new StringContent(ddlMenu.SelectedValue), "menuId");
                form.Add(new StringContent(ddlCategory.SelectedValue), "categoryId");

                if (string.IsNullOrEmpty(hfId.Value))
                {
                    client.PostAsync($"{ApiBaseUrl}/api/document/upload", form).Wait();
                }
                else
                {
                    client.PutAsync($"{ApiBaseUrl}/api/document/{hfId.Value}", form).Wait();
                }
            }

            Response.Redirect(Request.RawUrl);
        }

        protected void gvDocs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "deleteBaris")
            {
                DeleteDocument(id);
                LoadDocumentsSync();
            }
            else if (e.CommandName == "editBaris")
            {
                LoadDocumentByIdSync(id);
            }
        }

        private void DeleteDocument(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.DeleteAsync($"{ApiBaseUrl}/api/document/{id}").Result;
            }
        }

        private void LoadDocumentsSync()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync($"{ApiBaseUrl}/api/document/all").Result;

                var data = JsonConvert.DeserializeObject<List<DocumentDTO>>(res);

                gvDocs.DataSource = data;
                gvDocs.DataBind();
            }
        }

        private void LoadDocumentByIdSync(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync($"{ApiBaseUrl}/api/document/{id}").Result;

                dynamic d = JsonConvert.DeserializeObject(res);

                hfId.Value = d.Id.ToString();
                txtTitle.Text = d.Title;

                if (ddlMenu.Items.FindByValue(d.MenuId.ToString()) != null)
                    ddlMenu.SelectedValue = d.MenuId.ToString();

                if (ddlCategory.Items.FindByValue(d.CategoryId.ToString()) != null)
                    ddlCategory.SelectedValue = d.CategoryId.ToString();
            }
        }

        private async Task LoadMenus()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");
                var data = JsonConvert.DeserializeObject<List<DocumentDTO>>(res);

                ddlMenu.DataSource = data;
                ddlMenu.DataTextField = "Name";
                ddlMenu.DataValueField = "Id";
                ddlMenu.DataBind();
            }
        }

        private async Task LoadCategories()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/category");
                var data = JsonConvert.DeserializeObject<List<DocumentDTO>>(res);

                ddlCategory.DataSource = data;
                ddlCategory.DataTextField = "Name";
                ddlCategory.DataValueField = "Id";
                ddlCategory.DataBind();
            }
        }

        /// <summary>
        /// Dipakai dari markup: <%# GetPreviewUrl(Eval("Id")) %>
        /// File sekarang disimpan di D:\SOP\Document\ lewat FileStorageHelper,
        /// jadi tidak lagi diakses lewat "ApiBaseUrl + rawPath", melainkan
        /// lewat endpoint streaming api/document/preview/{id}.
        /// </summary>
        public string GetPreviewUrl(object id)
        {
            if (id == null) return "#";

            string username = Session["User"]?.ToString() ?? "";
            string encodedUser = HttpUtility.UrlEncode(username);

            return $"{ApiBaseUrl.TrimEnd('/')}/api/document/preview/{id}?username={encodedUser}";
        }
    }
}
