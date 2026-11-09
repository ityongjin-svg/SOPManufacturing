using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI.Main
{
    public partial class MachineManagement : System.Web.UI.Page
    {
        string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"] + "/api/machine/";

        private int EditFileId
        {
            get { return ViewState["EditFileId"] == null ? 0 : (int)ViewState["EditFileId"]; }
            set { ViewState["EditFileId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");

                if (categoryId == 12)
                {
                    pnlMaster.Visible = true;
                    pnlMachine.Visible = false;

                    LoadItems();   // popup/grid master
                    LoadMasterFiles();
                }
                else
                {
                    pnlMaster.Visible = false;
                    pnlMachine.Visible = true;

                    LoadMachineList(); // existing function
                }
            }
        }

        // =========================
        // LOAD LIST
        // =========================
        private void LoadMachineList()
        {
            using (HttpClient client = new HttpClient())
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");

                var res = client.GetStringAsync(baseUrl + $"?categoryId={categoryId}").Result;

                var data = JsonConvert.DeserializeObject<List<MachineItemDTO>>(res);

                // 🔥 FLATTEN
                var flatList = new List<MachineFlatDTO>();

                foreach (var item in data)
                {
                    foreach (var type in item.Types)
                    {
                        foreach (var file in type.Files)
                        {
                            flatList.Add(new MachineFlatDTO
                            {
                                Id = file.Id,
                                TypeId = type.Id,
                                ItemName = item.ItemName,
                                TypeName = type.TypeName,
                                FileName = file.FileName,
                                FilePath = file.FilePath
                            });
                        }
                    }
                }

                gvMachine.DataSource = flatList;
                gvMachine.DataBind();
            }
        }

        // =========================
        // UPLOAD / SAVE
        // =========================
        protected async void btnUpload_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                var form = new MultipartFormDataContent();
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");

                if (fuMachineFile.HasFile)
                {
                    var fileContent = new StreamContent(fuMachineFile.FileContent);
                    form.Add(fileContent, "file", fuMachineFile.FileName);
                }

                if (EditFileId == 0)
                {
                    form.Add(new StringContent(txtItem.Text), "ItemName");
                    form.Add(new StringContent(txtType.Text), "TypeName");
                    form.Add(new StringContent(categoryId.ToString()), "CategoryId");
                    await client.PostAsync(baseUrl + "upload", form);
                }
                else
                {
                    form.Add(new StringContent(EditFileId.ToString()), "Id");
                    form.Add(new StringContent(hfTypeId.Value), "TypeId");
                    await client.PostAsync(baseUrl + "update", form);
                }
            }

            ResetForm();
            LoadMachineList();
        }

        // =========================
        // GRID ACTION
        // =========================
        protected void gvMachine_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "deleteRow")
            {
                int id = Convert.ToInt32(gvMachine.DataKeys[index].Value);

                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"file/{id}").Wait();
                }

                LoadMachineList();
            }

            if (e.CommandName == "editRow")
            {
                int id = Convert.ToInt32(gvMachine.DataKeys[index].Values["Id"]);
                int typeId = Convert.ToInt32(gvMachine.DataKeys[index].Values["TypeId"]);

                EditFileId = id;
                hfTypeId.Value = typeId.ToString();

                txtItem.Text = gvMachine.Rows[index].Cells[0].Text;
                txtType.Text = gvMachine.Rows[index].Cells[1].Text;

                btnUpload.Text = "Update";
            }
        }

        // =========================
        // RESET
        // =========================
        private void ResetForm()
        {
            txtItem.Text = "";
            txtType.Text = "";
            EditFileId = 0;
            hfTypeId.Value = "";

            btnUpload.Text = "Upload";
        }

        // Dipakai dari markup: <%# GetFileUrl(Eval("Id")) %>
        // File sekarang disimpan di D:\SOP\Document\, jadi tidak lagi
        // diakses lewat path mentah, melainkan lewat endpoint streaming API.
        public string GetFileUrl(object id)
        {
            if (id == null) return "#";
            string apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
            return $"{apiBaseUrl}/api/machine/download/{id}";
        }

        private void LoadItemPopup()
        {
            using (HttpClient client = new HttpClient())
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");

                var res = client.GetStringAsync(baseUrl + $"/items?categoryId={categoryId}").Result;

                var data = JsonConvert.DeserializeObject<List<Machine_Items>>(res);

                gvItems.DataSource = data;
                gvItems.DataBind();
            }
        }

        protected void btnOpenPopup_Click(object sender, EventArgs e)
        {
            pnlItemPopup.Visible = true;
            LoadItemPopup();
        }

        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandArgument == null) return;

            int index = Convert.ToInt32(e.CommandArgument);

            // safety check
            if (index < 0 || index >= gvItems.Rows.Count) return;

            int id = Convert.ToInt32(gvItems.DataKeys[index].Value);

            if (e.CommandName == "selectItem")
            {
                string name = gvItems.Rows[index].Cells[0].Text;

                txtItem.Text = name;
                hfItemId.Value = id.ToString();
                pnlItemPopup.Visible = false;
            }

            else if (e.CommandName == "editItem")
            {
                txtItemNameModal.Text = gvItems.Rows[index].Cells[0].Text;
                txtSortModal.Text = gvItems.Rows[index].Cells[1].Text;

                ViewState["EditItemId"] = id;
            }

            else if (e.CommandName == "deleteItem")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"item/{id}").Wait();
                }

                LoadItems();
            }
        }

        private void LoadItems()
        {
            using (HttpClient client = new HttpClient())
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");

                var res = client.GetStringAsync(baseUrl + $"items?categoryId={categoryId}").Result;

                var data = JsonConvert.DeserializeObject<List<Machine_Items>>(res);

                gvItems.DataSource = data;
                gvItems.DataBind();
            }
        }

        protected void btnSaveItem_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                int categoryId = int.Parse(Request.QueryString["categoryId"] ?? "12");
                int editId = ViewState["EditItemId"] == null ? 0 : (int)ViewState["EditItemId"];

                var data = new
                {
                    Id = editId,
                    ItemName = txtItemNameModal.Text,
                    SortOrder = txtSortModal.Text,
                    CategoryId = categoryId,
                    IsActive = true
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (editId == 0)
                    client.PostAsync(baseUrl + "item", content).Wait();
                else
                    client.PutAsync(baseUrl + "item", content).Wait();
            }

            ViewState["EditItemId"] = 0;

            LoadItems(); // refresh grid
        }

        protected void btnClosePopup_Click(object sender, EventArgs e)
        {
            pnlItemPopup.Visible = false;
        }

        private int EditTypeId
        {
            get { return ViewState["EditTypeId"] == null ? 0 : (int)ViewState["EditTypeId"]; }
            set { ViewState["EditTypeId"] = value; }
        }

        private void LoadTypes(int itemId)
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + $"types?itemId={itemId}").Result;

                var data = JsonConvert.DeserializeObject<List<Machine_Types>>(res);

                gvTypePopup.DataSource = data;
                gvTypePopup.DataBind();
            }
        }

        protected void gvTypePopup_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvTypePopup.DataKeys[index].Value);

            if (e.CommandName == "selectType")
            {
                string name = gvTypePopup.Rows[index].Cells[0].Text;

                txtType.Text = name;
                hfTypeId.Value = id.ToString();

                typeModal.Visible = false;
            }

            if (e.CommandName == "editType")
            {
                txtTypeNameModal.Text = gvTypePopup.Rows[index].Cells[0].Text;
                txtTypeSortModal.Text = gvTypePopup.Rows[index].Cells[1].Text;

                EditTypeId = id;   // GANTI dari hfTypeId.Value = id.ToString();
            }

            if (e.CommandName == "deleteType")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"type/{id}").Wait();
                }

                LoadTypes(int.Parse(hfItemId.Value));   // GANTI dari LoadTypes();
            }
        }

        protected async void btnSaveType_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new
                {
                    Id = EditTypeId,                       // GANTI dari hfTypeId.Value
                    ItemId = int.Parse(hfItemId.Value),
                    TypeName = txtTypeNameModal.Text,
                    SortOrder = int.Parse(txtTypeSortModal.Text == "" ? "0" : txtTypeSortModal.Text),
                    IsActive = true
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (data.Id == 0)
                    await client.PostAsync(baseUrl + "type", content);
                else
                    await client.PutAsync(baseUrl + "type", content);
            }

            EditTypeId = 0;                                 // reset edit state, BUKAN hfTypeId
            txtTypeNameModal.Text = "";
            txtTypeSortModal.Text = "";

            LoadTypes(int.Parse(hfItemId.Value));            // GANTI dari LoadTypes();
        }

        protected void btnOpenType_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfItemId.Value))
                return;

            typeModal.Visible = true;
            LoadTypes(int.Parse(hfItemId.Value));
        }

        private void LoadMasterFiles()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetStringAsync(baseUrl + "/master/files").Result;

                var data = JsonConvert.DeserializeObject<List<MachineFile>>(res);

                gvMasterFiles.DataSource = data;
                gvMasterFiles.DataBind();
            }
        }

        protected void btnSaveMasterFile_Click(object sender, EventArgs e)
        {
            if (!fuMasterFile.HasFile)
                return;

            using (HttpClient client = new HttpClient())
            {
                var form = new MultipartFormDataContent();

                var fileContent = new StreamContent(fuMasterFile.FileContent);
                form.Add(fileContent, "file", fuMasterFile.FileName);

                // API sekarang yang menyimpan file fisiknya ke D:\SOP\Document\{Tipe}\
                client.PostAsync(baseUrl + "master/file", form).Wait();
            }

            LoadMasterFiles();
        }

        protected void gvMasterFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int id = Convert.ToInt32(gvMasterFiles.DataKeys[index].Value);

            if (e.CommandName == "deleteFile")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DeleteAsync(baseUrl + $"master/file/{id}").Wait();
                }

                LoadMasterFiles();
            }
        }

        protected void btnCloseType_Click(object sender, EventArgs e)
        {
            typeModal.Visible = false;
        }

        public bool IsPreviewable(object fileName)
        {
            string ext = System.IO.Path.GetExtension((fileName ?? "").ToString()).ToLower();
            string[] types = { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
            return types.Contains(ext);
        }

        public string GetPreviewUrl(object id)
        {
            return GetFileUrl(id) + "?mode=preview";
        }
    }
}