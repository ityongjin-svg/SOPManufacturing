using System;
using System.Web;
using System.Web.UI;

namespace SOP_UI
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Hapus semua data sesi (Session["User"], Session["AccessSuper"], dll)
            Session.Clear();
            Session.Abandon();

            // Hapus cookie session di browser supaya request berikutnya benar-benar
            // dianggap belum login (bukan cuma kosong di server).
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                var cookie = new HttpCookie("ASP.NET_SessionId", "")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            // Cegah halaman-halaman yang sudah di-render sebelumnya muncul lagi
            // lewat tombol Back browser (browser dipaksa ambil ulang dari server,
            // dan server akan redirect ke Login karena Session["User"] sudah kosong).
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            Response.Redirect("~/Login.aspx", true);
        }
    }
}
