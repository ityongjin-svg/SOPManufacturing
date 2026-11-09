using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI.Main
{
    public partial class MenuManagement : System.Web.UI.Page
    {
        private string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadMenus();
                    await LoadParentDropdown();
                }));
            }
        }

        private async Task LoadMenus()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");

                var data = JsonConvert.DeserializeObject<List<MenuDTO>>(res);

                gvMenu.DataSource = data;
                gvMenu.DataBind();
            }
        }

        private async Task LoadParentDropdown()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu");

                var data = JsonConvert.DeserializeObject<List<MenuDTO>>(res);

                ddlParent.DataSource = data;
                ddlParent.DataTextField = "Name";
                ddlParent.DataValueField = "Id";
                ddlParent.DataBind();

                ddlParent.Items.Insert(0, new ListItem("-- Root --", ""));
            }
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            var menu = new
            {
                Name = txtName.Text,
                ParentId = string.IsNullOrEmpty(ddlParent.SelectedValue) ? (int?)null : int.Parse(ddlParent.SelectedValue),
                Level = int.Parse(txtLevel.Text),
                Url = txtUrl.Text,
                SortOrder = int.Parse(txtSort.Text)
            };

            using (HttpClient client = new HttpClient())
            {
                if (string.IsNullOrEmpty(hfId.Value))
                {
                    await client.PostAsJsonAsync($"{ApiBaseUrl}/api/menu", menu);
                }
                else
                {
                    await client.PutAsJsonAsync($"{ApiBaseUrl}/api/menu/{hfId.Value}", menu);
                }
            }

            ClearForm();

            await LoadMenus();
            await LoadParentDropdown();
        }

        protected void gvMenu_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "editMenu")
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadMenuById(id);
                }));
            }
            else if (e.CommandName == "deleteMenu")
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await DeleteMenu(id);
                }));
            }
        }

        private async Task LoadMenuById(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync($"{ApiBaseUrl}/api/menu/{id}");

                var data = JsonConvert.DeserializeObject<MenuDTO>(res);

                hfId.Value = data.Id.ToString();
                txtName.Text = data.Name;
                txtLevel.Text = data.Level.ToString();
                txtUrl.Text = data.Url;
                txtSort.Text = data.SortOrder.ToString();

                if (data.ParentId != null)
                    ddlParent.SelectedValue = data.ParentId.ToString();
            }
        }

        private async Task DeleteMenu(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                await client.DeleteAsync($"{ApiBaseUrl}/api/menu/{id}");
            }

            await LoadMenus();
        }

        private void ClearForm()
        {
            hfId.Value = "";
            txtName.Text = "";
            txtUrl.Text = "";
            txtLevel.Text = "";
            txtSort.Text = "";
            ddlParent.SelectedIndex = 0;
        }
    }
}
