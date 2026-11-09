using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI.Main
{
    public partial class SOPManagement : System.Web.UI.Page
    {
        string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"] + "/api/";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMenusAll();

                LoadSectionList();
                LoadGroupList();
                LoadItemList();
                LoadCategories();
            }
        }
        private int EditSectionId
        {
            get { return ViewState["EditSectionId"] == null ? 0 : (int)ViewState["EditSectionId"]; }
            set { ViewState["EditSectionId"] = value; }
        }

        private int EditGroupId
        {
            get { return ViewState["EditGroupId"] == null ? 0 : (int)ViewState["EditGroupId"]; }
            set { ViewState["EditGroupId"] = value; }
        }

        private int EditItemId
        {
            get { return ViewState["EditItemId"] == null ? 0 : (int)ViewState["EditItemId"]; }
            set { ViewState["EditItemId"] = value; }
        }

        // ================= MENU LOAD =================
        private void LoadMenusAll()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "menu")
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                var data = JsonConvert.DeserializeObject<List<MenuDTO>>(res)
                    .Where(x => x.Name.Contains("SOP"))
                    .ToList();

                BindDropdown(ddlMenuSection, data);
                BindDropdown(ddlMenuGroup, data);
                BindDropdown(ddlMenuItem, data);
            }
        }

        private void BindDropdown(DropDownList ddl, List<MenuDTO> data)
        {
            ddl.DataSource = data;
            ddl.DataTextField = "Name";
            ddl.DataValueField = "Id";
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem("-- Select --", "0"));
        }

        // ================= SECTION =================

        protected async void btnSaveSection_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StringContent(txtSectionName.Text), "Name");
                form.Add(new StringContent(ddlMenuSection.SelectedValue), "MenuId");

                if (fuSectionImage.HasFile)
                {
                    var fileContent = new StreamContent(fuSectionImage.FileContent);
                    form.Add(fileContent, "image", fuSectionImage.FileName);
                }
                form.Add(new StringContent(txtSort.Text), "SortOrder");

                if (EditSectionId == 0)
                    await client.PostAsync(baseUrl + "sop/section", form);
                else
                    await client.PutAsync(baseUrl + $"sop/section/{EditSectionId}", form);
            }

            EditSectionId = 0;
            btnSaveSection.Text = "Save Section";

            LoadSectionList();
        }

        // ================= GROUP =================

        protected async void ddlMenuGroup_Changed(object sender, EventArgs e)
        {
            int menuId = int.Parse(ddlMenuGroup.SelectedValue);
            await LoadSections(menuId, ddlSectionGroup);
        }

        protected async void btnSaveGroup_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new
                {
                    Id = EditGroupId,
                    Name = txtGroupName.Text,
                    SectionId = ddlSectionGroup.SelectedValue
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (EditGroupId == 0)
                    await client.PostAsync(baseUrl + "sop/group", content);
                else
                    await client.PutAsync(baseUrl + $"sop/group/{EditGroupId}", content);
            }

            EditGroupId = 0;
            btnSaveGroup.Text = "Save Group";

            LoadGroupList();
        }

        // ================= ITEM =================

        protected async void ddlMenuItem_Changed(object sender, EventArgs e)
        {
            int menuId = int.Parse(ddlMenuItem.SelectedValue);
            await LoadSections(menuId, ddlSectionItem);
        }

        protected async void ddlSectionItem_Changed(object sender, EventArgs e)
        {
            int sectionId = int.Parse(ddlSectionItem.SelectedValue);
            await LoadGroups(sectionId, ddlGroupItem);
        }

        protected async void btnSaveItem_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                string fileName = "";
                string filePath = "";
                string fileType = "";

                // =========================
                // UPLOAD FILE KE API DULU (bukan simpan lokal UI lagi)
                // =========================
                if (fuSOPFile.HasFile)
                {
                    using (var uploadForm = new MultipartFormDataContent())
                    {
                        var fileContent = new StreamContent(fuSOPFile.FileContent);
                        uploadForm.Add(fileContent, "file", fuSOPFile.FileName);

                        var uploadRes = await client.PostAsync(baseUrl + "sop/item/file", uploadForm);
                        var uploadJson = await uploadRes.Content.ReadAsStringAsync();

                        dynamic uploadResult = JsonConvert.DeserializeObject(uploadJson);

                        fileName = uploadResult.fileName;
                        filePath = uploadResult.filePath;
                        fileType = uploadResult.fileType;
                    }
                }

                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StringContent(EditItemId.ToString()), "id");
                form.Add(new StringContent(txtItemTitle.Text), "itemTitle");
                form.Add(new StringContent(ddlMenuItem.SelectedValue), "menuId");
                form.Add(new StringContent(ddlSectionItem.SelectedValue), "sectionId");
                form.Add(new StringContent(ddlGroupItem.SelectedValue), "groupId");

                form.Add(new StringContent(
                    string.IsNullOrEmpty(ddlCategory.SelectedValue) ? "" : ddlCategory.SelectedValue
                ), "categoryId");

                form.Add(new StringContent(fileName), "fileName");
                form.Add(new StringContent(filePath), "filePath");
                form.Add(new StringContent(fileType), "fileType");

                form.Add(new StringContent(Session["User"]?.ToString() ?? "admin"), "createdBy");

                if (EditItemId == 0)
                {
                    await client.PostAsync(baseUrl + "sop/upload", form);
                }
                else
                {
                    await client.PutAsync(baseUrl + $"sop/item/{EditItemId}", form);
                }
            }

            EditItemId = 0;
            btnSaveItem.Text = "Upload SOP";

            ResetForm();
            LoadItemList();

            Response.Redirect(Request.RawUrl);
        }

        // ================= SHARED =================

        private async Task LoadSections(int menuId, DropDownList ddl)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync(baseUrl + $"sop/section?menuId={menuId}");

                var data = JsonConvert.DeserializeObject<List<SectionDTO>>(res);

                ddl.DataSource = data;
                ddl.DataTextField = "Name";
                ddl.DataValueField = "Id";
                ddl.DataBind();

                ddl.Items.Insert(0, new ListItem("-- Select Section --", "0"));
            }
        }

        private async Task LoadGroups(int sectionId, DropDownList ddl)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync(baseUrl + $"sop/group?sectionId={sectionId}");

                var data = JsonConvert.DeserializeObject<List<GroupDTO>>(res);

                ddl.DataSource = data;
                ddl.DataTextField = "Name";
                ddl.DataValueField = "Id";
                ddl.DataBind();

                ddl.Items.Insert(0, new ListItem("-- Select Group --", "0"));
            }
        }

        private void LoadSectionList()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "sop/section/all").Result;

                var data = JsonConvert.DeserializeObject<List<SectionDTO>>(res);

                gvSection.DataSource = data;
                gvSection.DataBind();
            }
        }

        private void LoadGroupList()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "sop/group/all").Result;

                var data = JsonConvert.DeserializeObject<List<GroupDTO>>(res);

                gvGroup.DataSource = data;
                gvGroup.DataBind();
            }
        }

        private void LoadItemList()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "sop/item/all").Result;

                var data = JsonConvert.DeserializeObject<List<ItemDTO>>(res);

                gvItem.DataSource = data;
                gvItem.DataBind();
            }
        }

        protected void gvSection_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvSection.DataKeys[index].Value);

            if (e.CommandName == "editBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    var res = client.GetStringAsync(baseUrl + $"sop/section/{id}").Result;

                    var item = JsonConvert.DeserializeObject<SectionDTO>(res);
                    EditSectionId = id;

                    txtSort.Text = item.SortOrder.ToString();
                    txtSectionName.Text = item.Name;
                    // tambahkan di dalam blok editBaris, setelah txtSectionName.Text = item.Name;
                    if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        imgCurrentSection.ImageUrl = GetImageUrl(item.ImagePath);
                        pnlCurrentImage.Visible = true;
                    }
                    else
                    {
                        pnlCurrentImage.Visible = false;
                    }
                    ddlMenuSection.SelectedValue = item.MenuId.ToString();
                    btnSaveSection.Text = "Update Section";
                }
            }

            if (e.CommandName == "deleteBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"sop/section/{id}").Wait();
                }

                LoadSectionList();
            }
        }

        protected void gvGroup_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvGroup.DataKeys[index].Value);

            if (e.CommandName == "editBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    var res = client.GetStringAsync(baseUrl + $"sop/group/{id}").Result;

                    var item = JsonConvert.DeserializeObject<GroupDTO>(res);
                    EditGroupId = id;

                    txtGroupName.Text = gvGroup.Rows[index].Cells[0].Text;
                    ddlMenuGroup.SelectedValue = item.MenuId.ToString();

                    LoadSectionsGroupSync(item.MenuId);
                    if (ddlSectionGroup.Items.FindByValue(item.SectionId.ToString()) != null)
                        ddlSectionGroup.SelectedValue = item.SectionId.ToString();

                    btnSaveGroup.Text = "Update Group";
                }
            }

            if (e.CommandName == "deleteBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"sop/group/{id}").Wait();
                }

                LoadGroupList();
            }
        }

        private void LoadSectionsItemSync(int menuId)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync($"{baseUrl}sop/section?menuId={menuId}").Result;

                var data = JsonConvert.DeserializeObject<List<SectionDTO>>(res);

                ddlSectionItem.DataSource = data;
                ddlSectionItem.DataTextField = "Name";
                ddlSectionItem.DataValueField = "Id";
                ddlSectionItem.DataBind();

                ddlSectionItem.Items.Insert(0, new ListItem("-- Select Section --", "0"));
            }
        }

        private void LoadSectionsGroupSync(int menuId)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync($"{baseUrl}sop/section?menuId={menuId}").Result;

                var data = JsonConvert.DeserializeObject<List<SectionDTO>>(res);

                ddlSectionGroup.DataSource = data;
                ddlSectionGroup.DataTextField = "Name";
                ddlSectionGroup.DataValueField = "Id";
                ddlSectionGroup.DataBind();

                ddlSectionGroup.Items.Insert(0, new ListItem("-- Select Section --", "0"));
            }
        }

        private void LoadGroupsSync(int sectionId)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync($"{baseUrl}sop/group?sectionId={sectionId}").Result;

                var data = JsonConvert.DeserializeObject<List<GroupDTO>>(res);

                ddlGroupItem.DataSource = data;
                ddlGroupItem.DataTextField = "Name";
                ddlGroupItem.DataValueField = "Id";
                ddlGroupItem.DataBind();

                ddlGroupItem.Items.Insert(0, new ListItem("-- Select Group --", "0"));
            }
        }

        protected void gvItem_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvItem.DataKeys[index].Value);

            if (e.CommandName == "editBaris")
            {
                EditItemId = id;

                using (HttpClient client = new HttpClient())
                {
                    var res = client.GetStringAsync(baseUrl + $"sop/item/{id}").Result;

                    var item = JsonConvert.DeserializeObject<ItemDTO>(res);

                    txtItemTitle.Text = item.ItemTitle;

                    ddlMenuItem.SelectedValue = item.MenuId.ToString();

                    LoadSectionsItemSync(item.MenuId);
                    if (ddlSectionItem.Items.FindByValue(item.SectionId.ToString()) != null)
                        ddlSectionItem.SelectedValue = item.SectionId.ToString();

                    LoadGroupsSync(item.SectionId);

                    if (ddlGroupItem.Items.FindByValue(item.GroupId.ToString()) != null)
                        ddlGroupItem.SelectedValue = item.GroupId.ToString();
                    if (item.CategoryId != null)
                    {
                        string cat = item.CategoryId.ToString();

                        if (ddlCategory.Items.FindByValue(cat) != null)
                            ddlCategory.SelectedValue = cat;
                    }
                    btnSaveItem.Text = "Update SOP";
                }
            }

            if (e.CommandName == "deleteBaris")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"sop/item/{id}").Wait();
                }

                LoadItemList();
            }
        }

        private void LoadCategories()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "sop/category").Result;

                var data = JsonConvert.DeserializeObject<List<CategoryDTO>>(res);

                ddlCategory.DataSource = data;
                ddlCategory.DataTextField = "Name";
                ddlCategory.DataValueField = "Id";
                ddlCategory.DataBind();

                ddlCategory.Items.Insert(0, new ListItem("-- Optional Category --", ""));
            }
        }

        private void ResetForm()
        {
            txtSort.Text = "";
            txtSectionName.Text = "";
            txtGroupName.Text = "";
            txtItemTitle.Text = "";

            pnlCurrentImage.Visible = false;

            EditSectionId = 0;
            EditGroupId = 0;
            EditItemId = 0;

            btnSaveSection.Text = "Save Section";
            btnSaveGroup.Text = "Save Group";
            btnSaveItem.Text = "Upload SOP";
        }

        public string GetImageUrl(object id)
        {
            if (id == null) return "";

            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];

            return $"{baseUrl}/api/sop/section/image/{id}";
        }
    }
}