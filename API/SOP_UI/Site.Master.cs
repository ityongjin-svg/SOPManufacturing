using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SOP_UI
{
    public partial class Site : MasterPage
    {
        private List<MenuDTO> allMenus;

        // Path halaman yang sedang dibuka (lowercase), dipakai BuildMenu()
        // untuk kasih tanda "active" ke menu yang sesuai.
        private string _currentPath;

        private static readonly string[] SuperOnlyPages =
        {
            "/menumanagement.aspx",
            "/documentmanagement.aspx",
            "/sopmanagement.aspx",
            "/machinemanagement.aspx"
        };

        private static readonly string[] QCManagementPages =
        {
            "/qclayoutmanagement.aspx"
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            string path = Request.Url.AbsolutePath.ToLower();
            _currentPath = path;

            if (path.EndsWith("/login.aspx") || path.EndsWith("/logout.aspx"))
                return;

            if (Session["User"] == null)
            {
                string returnUrl = Server.UrlEncode(Request.Url.PathAndQuery);
                Response.Redirect($"~/Login.aspx?ReturnUrl={returnUrl}", true);
                return;
            }

            // Default.aspx / root sudah tidak dipakai sebagai landing page —
            // begitu user sudah login, selalu arahkan ke Main/Home.
            if (path.EndsWith("/default.aspx") || path == "/default" || path == "/")
            {
                Response.Redirect("~/Main/Home.aspx", true);
                return;
            }

            bool isSuper = Session["AccessSuper"] != null && (bool)Session["AccessSuper"];
            string currentUser = Session["User"]?.ToString() ?? "";
            bool hasQCAccess = isSuper || CheckQCAccess(currentUser);

            // Halaman admin selain QC Layout Management tetap khusus Super
            if (!isSuper && SuperOnlyPages.Any(p => path.EndsWith(p)))
            {
                Response.Redirect("~/Main/Home.aspx", true);
                return;
            }

            // Halaman QC Layout Management: boleh Super ATAU yang ACCESS=1
            if (!hasQCAccess && QCManagementPages.Any(p => path.EndsWith(p)))
            {
                Response.Redirect("~/Main/Home.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                litUsername.Text = Session["User"]?.ToString() ?? "";
                LoadMenu(isSuper, hasQCAccess);

                if (path.Contains("home") || path.Contains("default.aspx"))
                {
                    navbar.Attributes["class"] += " navbar-transparent";
                    bodyTag.Attributes["class"] = "home-page";
                }
            }
        }

        private bool CheckQCAccess(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;

            string apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];

            using (HttpClient client = new HttpClient())
            {
                string url = $"{apiBaseUrl.TrimEnd('/')}/api/access/checkqc?username={HttpUtility.UrlEncode(username)}";
                var res = client.GetStringAsync(url).Result;
                dynamic result = JsonConvert.DeserializeObject(res);
                return (bool)result.hasAccess;
            }
        }

        private void LoadMenu(bool isSuper, bool hasQCAccess)
        {
            string apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];

            using (HttpClient client = new HttpClient())
            {
                string url = $"{apiBaseUrl.TrimEnd('/')}/api/menu";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        allMenus = JsonConvert.DeserializeObject<List<MenuDTO>>(data);
                        if (allMenus != null)
                        {
                            if (!isSuper)
                            {
                                if (hasQCAccess)
                                {
                                    // Bukan Super, tapi ACCESS=1: tampilkan root DASHBOARD +
                                    // hanya submenu QC Layout Management, sembunyikan sisanya.
                                    allMenus = allMenus
                                        .Where(m => !IsDashboardOrDescendant(m, allMenus)
                                                    || IsDashboardRootOrQCLayoutItem(m))
                                        .ToList();
                                }
                                else
                                {
                                    // Bukan Super dan tidak ACCESS: sembunyikan seluruh DASHBOARD.
                                    allMenus = allMenus
                                        .Where(m => !IsDashboardOrDescendant(m, allMenus))
                                        .ToList();
                                }
                            }
                            // isSuper == true: tidak difilter sama sekali, semua tampil.

                            menuContainer.InnerHtml = BuildMenu(null, 0);
                        }
                    }
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    System.Diagnostics.Debug.WriteLine($"API Error {(int)response.StatusCode}: {errorContent}");
                    menuContainer.InnerHtml = "<!-- Fail to load menu -->";
                }
            }
        }

        private bool IsDashboardRootOrQCLayoutItem(MenuDTO menu)
        {
            // Root menu DASHBOARD itu sendiri harus tetap tampil sebagai "wadah"
            if (menu.ParentId == null &&
                string.Equals(menu.Name, "DASHBOARD", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Item submenu QC Layout Management
            if (!string.IsNullOrEmpty(menu.Url) &&
                menu.Url.ToLower().Contains("qclayoutmanagement.aspx"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// True kalau menu ini sendiri, atau salah satu leluhurnya (parent /
        /// grandparent), adalah menu root bernama "DASHBOARD".
        /// </summary>
        private bool IsDashboardOrDescendant(MenuDTO menu, List<MenuDTO> all)
        {
            var current = menu;

            while (current != null)
            {
                if (current.ParentId == null &&
                    string.Equals(current.Name, "DASHBOARD", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current = current.ParentId.HasValue
                    ? all.FirstOrDefault(x => x.Id == current.ParentId.Value)
                    : null;
            }

            return false;
        }

        private string BuildMenu(int? parentId, int level = 0)
        {
            var items = allMenus
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.SortOrder)
                .ToList();

            if (!items.Any())
                return "";

            StringBuilder sb = new StringBuilder();

            foreach (var item in items)
            {
                var children = allMenus.Any(x => x.ParentId == item.Id);

                string label = BuildLabel(item.Name, item.NameKr);

                // "active" = link ini yang lagi dibuka, atau salah satu anaknya yang lagi dibuka
                bool isActive = IsCurrentUrl(item.Url);
                bool hasActiveChild = children && HasActiveDescendant(item.Id);
                string activeClass = (isActive || hasActiveChild) ? " active" : "";

                if (level == 0)
                {
                    // LEVEL 1 — tetap satu baris (navbar utama), tidak perlu subtitle
                    if (children)
                    {
                        sb.Append("<li class='nav-item dropdown'>");
                        sb.Append($"<a class='nav-link dropdown-toggle{activeClass}' href='#' data-bs-toggle='dropdown'>{item.Name}</a>");
                        sb.Append("<ul class='dropdown-menu'>");
                        sb.Append(BuildMenu(item.Id, level + 1));
                        sb.Append("</ul></li>");
                    }
                    else
                    {
                        sb.Append($"<li class='nav-item'><a class='nav-link{activeClass}' href='{item.Url}'>{item.Name}</a></li>");
                    }
                }
                else
                {
                    // LEVEL 2 / 3 — dua baris kalau ada NameKr
                    if (children)
                    {
                        sb.Append("<li class='dropdown-submenu'>");
                        sb.Append($"<a class='dropdown-item dropdown-toggle{activeClass}' href='#'>{label}<span class='submenu-arrow'>▸</span></a>");
                        sb.Append("<ul class='dropdown-menu'>");
                        sb.Append(BuildMenu(item.Id, level + 1));
                        sb.Append("</ul></li>");
                    }
                    else
                    {
                        sb.Append($"<li><a class='dropdown-item{activeClass}' href='{item.Url}'>{label}</a></li>");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>True kalau url menu ini cocok dengan halaman yang sedang dibuka.</summary>
        private bool IsCurrentUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(_currentPath))
                return false;

            return _currentPath.EndsWith(url.ToLower());
        }

        /// <summary>True kalau salah satu anak (langsung/cucu) dari menu ini sedang dibuka.</summary>
        private bool HasActiveDescendant(int parentId)
        {
            var children = allMenus.Where(x => x.ParentId == parentId).ToList();

            foreach (var child in children)
            {
                if (IsCurrentUrl(child.Url))
                    return true;

                if (allMenus.Any(x => x.ParentId == child.Id) && HasActiveDescendant(child.Id))
                    return true;
            }

            return false;
        }

        private string BuildLabel(string name, string nameKr)
        {
            if (string.IsNullOrWhiteSpace(nameKr))
                return $"<span class='menu-label'><span class='menu-main'>{name}</span></span>";

            return $@"
                <span class='menu-label'>
                    <span class='menu-main'>{name}</span>
                    <span class='menu-sub'>{nameKr}</span>
                </span>
             ";
        }
    }
}