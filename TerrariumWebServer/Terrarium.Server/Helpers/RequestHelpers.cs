using System;
using System.Net.Http;
using System.Web;

namespace Terrarium.Server.Helpers
{
    /// <summary>
    /// Helper class to handle HttpRequests
    /// </summary>
    public class RequestHelpers
    {
        /// <summary>
        /// Returns the clients IP address. Usable from ApiControllers.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The IP address from the client machine.</returns>
        public static string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            throw new Exception("Client IP Address Not Found in HttpRequest");
        }

        public static string GetClientIpAddress(HttpContext context)
        {
            return context.Request.UserHostAddress;
        }

        public static string GetClientIpAddress(HttpRequest request)
        {
            return request.UserHostAddress;
        }
    }
}