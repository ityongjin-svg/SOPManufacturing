using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SOP_UI
{
    public partial class Login : System.Web.UI.Page
    {
        private string ApiBaseUrl => ConfigurationManager.AppSettings["ApiBaseUrl"];

        protected void Page_Load(object sender, EventArgs e)
        {
            // Cegah halaman login sendiri ke-cache di browser (konsisten dengan
            // semua halaman lain, lihat Site.master.cs)
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            // Kalau sudah login, tidak perlu ke halaman login lagi
            if (!IsPostBack && Session["User"] != null)
            {
                Response.Redirect("~/Main/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected async void btnLogin_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";

            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
            {
                lblMsg.Text = "Username dan Password wajib diisi.";
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                var payload = new
                {
                    Username = txtUser.Text.Trim(),
                    Password = txtPass.Text
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                try
                {
                    response = await client.PostAsync($"{ApiBaseUrl}/api/login", content);
                }
                catch
                {
                    lblMsg.Text = "Tidak dapat menghubungi server. Coba lagi.";
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    lblMsg.Text = "Tidak dapat menghubungi server. Coba lagi.";
                    return;
                }

                var resBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(resBody);

                bool success = (bool)result.success;

                if (!success)
                {
                    lblMsg.Text = "Username atau Password salah.";
                    return;
                }

                // Session["User"] dipakai di seluruh aplikasi (mis. SOPManagement.aspx.cs,
                // Site.master.cs) sebagai identitas user yang sedang login.
                Session["User"] = (string)result.username;
                Session["AccessSuper"] = (bool)result.accessSuper;

                string returnUrl = Request.QueryString["ReturnUrl"];
                string redirectTo = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/Main/Home.aspx";

                Response.Redirect(redirectTo, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}