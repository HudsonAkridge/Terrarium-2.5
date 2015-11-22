using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Terrarium.Server.DataModels;
using Terrarium.Server.Helpers;
using Terrarium.Server.Models;
using Terrarium.Server.Models.Peers;
using Terrarium.Server.Services;

namespace Terrarium.Server.Controllers
{
    /// <summary>
    /// The Peer Discovery Service is the central location where
    /// peers can announce their existence and get information about other
    /// peers.  The primary services here are registering a user's email address,
    /// getting peer counts and lists, and registering a peer.
    /// </summary>
    public class PeerDiscoveryController : TerrariumApiControllerBase
    {
        /// <summary>
        /// PerformanceCounter for all monitored performance parameters on the Discovery Web Service.
        /// </summary>
        private static PerformanceCounter discoveryAllPerformanceCounter = InstallerInfo.CreatePerformanceCounter("AllDiscovery");
        /// <summary>
        /// PerformanceCounter for all monitored unsuccessful with the Discovery Web Service.
        /// </summary>
        private static PerformanceCounter discoveryAllFailuresPerformanceCounter = InstallerInfo.CreatePerformanceCounter("AllDiscoveryErrors");
        /// <summary>
        /// PerformanceCounter for monitoring peer registrations with the Discovery Web Service.
        /// </summary>
        private static PerformanceCounter discoveryRegistrationPerformanceCounter = InstallerInfo.CreatePerformanceCounter("Registration");
        /// <summary>
        /// PerformanceCounter for monitoring failed peer registration attempts with the Discovery Web Service.
        /// </summary>
        private static PerformanceCounter discoveryRegistrationFailuresPerformanceCounter = InstallerInfo.CreatePerformanceCounter("RegistrationErrors");

        private readonly ITerrariumDbContext _context;

        public PeerDiscoveryController(ITerrariumDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a Terrarium client user into the server database.
        /// </summary>
        /// <param name="email">E-mail address of the Terrarium user</param>
        /// <returns>Boolean indicating success or failure of the user registration.</returns>
        [HttpPost]
        [Route("api/peers/users/register")]
        public HttpResponseMessage RegisterUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("No email address provided")
                });
            }

            try
            {
                var ipAddress = GetCurrentRequestIpAddress();
                _context.AddUser(new UserRegister
                {
                    Email = email, 
                    IPAddress = ipAddress
                });
                discoveryAllPerformanceCounter?.Increment();
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("RegisterUser", e.ToString());

                discoveryAllFailuresPerformanceCounter?.Increment();

                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(e.Message)
                });
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Obtains the number of peers currently connected to the Terrarium Server.
        /// </summary>
        /// <param name="version">String specifying the version number.</param>
        /// <param name="channel">String specifying the channel number.</param>
        /// <returns>Integer count of the number of peers for the specified version and channel number.</returns>
        [HttpGet]
        [Route("api/peers/count")]
        public int GetNumPeers(string version, string channel)
        {
            return 10;
        }

        /// <summary>
        /// Validates a peer connection.
        /// </summary>
        /// <returns>A string representing the "REMOTE_ADDR" attribute from the Web Application ServerVariables collection.</returns>
        [HttpGet]
        [Route("api/peers/validate")]
        public string ValidatePeer()
        {
            return RequestHelpers.GetClientIpAddress(Request);
        }

        /// <summary>
        /// Checks to see if a specific version is disabled or not.  Used by the client at start up.
        /// This allows an admin to totally shutdown a version.
        /// </summary>
        /// <param name="version">String specifying the version number.</param>
        /// <param name="errorMessage"></param>
        /// <returns>A PeerVersionResult object containing the results of the query</returns>
        [HttpGet]
        public bool IsVersionDisabled(string version, out string errorMessage)
        {
            return VersionChecker.IsVersionDisabled(version, out errorMessage);
        }

        /// <summary>
        /// Registers a peer connection with the Terrarium Server.
        /// </summary>
        /// <param name="version">String specifying the version number.</param>
        /// <param name="channel">String specifying the channel number.</param>
        /// <param name="guid">Guid (Globally Unique Identifier) for the peer connection.</param>
        /// <returns>A PeerRegisterResult object containing the registration result</returns>
        [HttpPost]
        [Route("api/peers/register")]
        public PeerRegisterResult RegisterMyPeerGetCountAndPeerList(string version, string channel, Guid guid)
        {
            var result = new PeerRegisterResult();

            result.Peers = null;
            result.Count = 0;
            result.Result = RegisterPeerResult.Success;

            return result;
        }
    }
}
