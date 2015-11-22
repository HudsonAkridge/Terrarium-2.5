using System.Net;
using System.Net.Http;
using System.Web.Http;
using Terrarium.Server.Helpers;

namespace Terrarium.Server.Controllers
{
    /// <summary>
    ///     Returns various informational messages from the server
    /// </summary>
    public class MessageController : TerrariumApiControllerBase
    {
        /// <summary>
        ///     Gets the welcome message for the server.
        /// </summary>
        /// <returns>The welcome message as stored in the web.config file or a stock one if we can't read it.</returns>
        [HttpGet]
        [Route("api/messages/welcome")]
        public HttpResponseMessage Welcome()
        {
            string message;

            try { message = ServerSettings.WelcomeMessage; }
            catch
            {
                message = "Welcome to .NET Terrarium!";
            }

            return Request.CreateResponse(HttpStatusCode.OK, message);
        }

        /// <summary>
        ///     Gets the message of the day from the server.
        /// </summary>
        /// <returns>The message of the day as stored in the web.config file or a stock one if we can't read it.</returns>
        [HttpGet]
        [Route("api/messages/daily")]
        public HttpResponseMessage Daily()
        {
            string message;

            try { message = ServerSettings.MOTD; }
            catch
            {
                message = "Have Fun!";
            }

            return Request.CreateResponse(HttpStatusCode.OK, message);
        }

        /// <summary>
        ///     Gets the latest version of the Terrarium SDK this server is using.
        /// </summary>
        /// <returns>The latest version as stored in the web.config file or a stock one if we can't read it.</returns>
        [HttpGet]
        [Route("api/version")]
        public HttpResponseMessage Version()
        {
            string message;

            try
            {
                // TODO find Terrarium.Sdk.dll and return that version here instead
                message = ServerSettings.LatestVersion;
            }
            catch
            {
                message = "1.0.0.0";
            }

            return Request.CreateResponse(HttpStatusCode.OK, message);
        }
    }
}