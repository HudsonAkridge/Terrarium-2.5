using System.Web;
using System.Web.Http;
using Terrarium.Server.Helpers;

namespace Terrarium.Server.Controllers
{
    public abstract class TerrariumApiControllerBase : ApiController
    {
        protected virtual HttpRequest CurrentRequest => HttpContext.Current.Request;

        protected virtual string GetCurrentRequestIpAddress()
        {
            return RequestHelpers.GetClientIpAddress(CurrentRequest);
        }

        protected virtual string GetCurrentRequestIpAddressFromServerVariables()
        {
            return CurrentRequest.ServerVariables["REMOTE_ADDR"];
        }
    }
}