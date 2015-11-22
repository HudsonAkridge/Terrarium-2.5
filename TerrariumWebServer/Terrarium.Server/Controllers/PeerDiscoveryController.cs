using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Terrarium.Server.DataModels;
using Terrarium.Server.Helpers;
using Terrarium.Server.Models;
using Terrarium.Server.Models.Peers;
using Terrarium.Server.Services;

namespace Terrarium.Server.Controllers
{
    /// <summary>
    ///     The Peer Discovery Service is the central location where
    ///     peers can announce their existence and get information about other
    ///     peers.  The primary services here are registering a user's email address,
    ///     getting peer counts and lists, and registering a peer.
    /// </summary>
    public class PeerDiscoveryController : TerrariumApiControllerBase
    {
        /// <summary>
        ///     PerformanceCounter for all monitored performance parameters on the Discovery Web Service.
        /// </summary>
        private static readonly PerformanceCounter DiscoveryAllPerformanceCounter = InstallerInfo.CreatePerformanceCounter("AllDiscovery");

        /// <summary>
        ///     PerformanceCounter for all monitored unsuccessful with the Discovery Web Service.
        /// </summary>
        private static readonly PerformanceCounter DiscoveryAllFailuresPerformanceCounter = InstallerInfo.CreatePerformanceCounter("AllDiscoveryErrors");

        /// <summary>
        ///     PerformanceCounter for monitoring peer registrations with the Discovery Web Service.
        /// </summary>
        private static readonly PerformanceCounter DiscoveryRegistrationPerformanceCounter = InstallerInfo.CreatePerformanceCounter("Registration");

        /// <summary>
        ///     PerformanceCounter for monitoring failed peer registration attempts with the Discovery Web Service.
        /// </summary>
        private static readonly PerformanceCounter DiscoveryRegistrationFailuresPerformanceCounter = InstallerInfo.CreatePerformanceCounter("RegistrationErrors");

        private readonly ITerrariumDbContext _context;

        public PeerDiscoveryController(ITerrariumDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Registers a Terrarium client user into the server database.
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
                DiscoveryAllPerformanceCounter?.Increment();
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("RegisterUser", e.ToString());

                DiscoveryAllFailuresPerformanceCounter?.Increment();

                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(e.Message)
                });
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        ///     Obtains the number of peers currently connected to the Terrarium Server.
        /// </summary>
        /// <param name="version">String specifying the version number.</param>
        /// <param name="channel">String specifying the channel number.</param>
        /// <returns>Integer count of the number of peers for the specified version and channel number.</returns>
        [HttpGet]
        [Route("api/peers/count")]
        public int GetNumPeers(string version, string channel)
        {
            if (channel == null || version == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("GetNumPeers", "Suspect: " + GetCurrentRequestIpAddress());

                DiscoveryAllFailuresPerformanceCounter?.Increment();

                return 0;
            }

            version = new Version(version).ToString(3);

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();

                    var mySqlCommand = new SqlCommand("TerrariumGrabNumPeers", myConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var parmVersion = mySqlCommand.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    parmVersion.Value = version;
                    var parmChannel = mySqlCommand.Parameters.Add("@Channel", SqlDbType.VarChar, 255);
                    parmChannel.Value = channel;

                    var count = mySqlCommand.ExecuteScalar();
                    DiscoveryAllPerformanceCounter?.Increment();

                    if (Convert.IsDBNull(count)) { return 0; }
                    return (int)count;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("GetNumPeers", e.ToString());

                DiscoveryAllFailuresPerformanceCounter?.Increment();

                return 0;
            }
        }

        /// <summary>
        ///     Validates a peer connection.
        /// </summary>
        /// <returns>A string representing the "REMOTE_ADDR" attribute from the Web Application ServerVariables collection.</returns>
        [HttpGet]
        [Route("api/peers/validate")]
        public string ValidatePeer()
        {
            DiscoveryAllPerformanceCounter?.Increment();

            return RequestHelpers.GetClientIpAddress(Request);
        }

        /// <summary>
        ///     Checks to see if a specific version is disabled or not.  Used by the client at start up.
        ///     This allows an admin to totally shutdown a version.
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
        /// <param name="peers">Dataset containing a list of peers.</param>
        /// <param name="count">Integer specifying the number of peers listed.</param>
        /// <returns>A possible value from the RegisterPeerResult enumeration.</returns>
        [HttpPost]
        [Route("api/peers/register")]
        public RegisterPeerResult RegisterMyPeerGetCountAndPeerList(string version, string channel, Guid guid, out DataSet peers, out int count)
        {
            peers = new DataSet();
            count = 0;

            if (channel == null || version == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("RegisterMyPeerGetCountAndPeerList", "Suspect: " + GetCurrentRequestIpAddress());

                DiscoveryAllFailuresPerformanceCounter?.Increment();

                return RegisterPeerResult.GlobalFailure;
            }

            var fullVersion = new Version(version).ToString(4);
            version = new Version(version).ToString(3);
            var ipAddress = GetCurrentRequestIpAddress();

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();

                    var command = new SqlCommand("TerrariumRegisterPeerCountAndList", myConnection);
                    var adapter = new SqlDataAdapter(command);
                    command.CommandType = CommandType.StoredProcedure;

                    var parmVersion = command.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    parmVersion.Value = version;
                    var parmFullVersion = command.Parameters.Add("@FullVersion", SqlDbType.VarChar, 255);
                    parmFullVersion.Value = fullVersion;
                    var parmChannel = command.Parameters.Add("@Channel", SqlDbType.VarChar, 255);
                    parmChannel.Value = channel;
                    var parmIp = command.Parameters.Add("@IPAddress", SqlDbType.VarChar, 50);
                    parmIp.Value = ipAddress;
                    var parmGuid = command.Parameters.Add("@Guid", SqlDbType.UniqueIdentifier, 16);
                    parmGuid.Value = guid;

                    var parmDisabledError = command.Parameters.Add("@Disabled_Error", SqlDbType.Bit, 1);
                    parmDisabledError.Direction = ParameterDirection.Output;
                    var parmPeerCount = command.Parameters.Add("@PeerCount", SqlDbType.Int, 4);
                    parmPeerCount.Direction = ParameterDirection.Output;

                    adapter.Fill(peers, "Peers");
                    count = (int)parmPeerCount.Value;

                    DiscoveryAllPerformanceCounter?.Increment();
                    DiscoveryRegistrationPerformanceCounter?.Increment();

                    if (((bool)parmDisabledError.Value)) { return RegisterPeerResult.GlobalFailure; }
                    return RegisterPeerResult.Success;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("RegisterMyPeerGetCountAndPeerList", e.ToString());

                DiscoveryRegistrationFailuresPerformanceCounter?.Increment();
                DiscoveryAllFailuresPerformanceCounter?.Increment();
            }

            return RegisterPeerResult.Failure;
        }
    }
}