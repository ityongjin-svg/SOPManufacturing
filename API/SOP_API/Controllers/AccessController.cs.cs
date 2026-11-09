using SOP_API.Helpers;
using System.Web.Http;

namespace SOP_API.Controllers
{
    [RoutePrefix("api/access")]
    public class AccessController : ApiController
    {
        [HttpGet]
        [Route("check")]
        public IHttpActionResult CheckAccess(string username)
        {
            return Ok(new { hasAccess = AccessHelper.HasDownloadAccess(username) });
        }

        [HttpGet]
        [Route("checkqc")]
        public IHttpActionResult CheckQCAccess(string username)
        {
            return Ok(new { hasAccess = AccessHelper.HasQCManagementAccess(username) });
        }
    }
}