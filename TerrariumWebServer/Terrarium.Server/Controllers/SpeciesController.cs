using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Http;
using Terrarium.Sdk.Enumerations;
using Terrarium.Server.Helpers;
using Terrarium.Server.Services;

namespace Terrarium.Server.Controllers
{
    /// <summary>
    ///     Encapsulates the functions required to insert new creatures into the
    ///     ecosystem and get creatures from the server during a reintroduction.
    /// </summary>
    public class SpeciesController : TerrariumApiControllerBase
    {
        /// <summary>
        ///     Gets a list of all species that have been blacklisted.
        /// </summary>
        /// <returns>The list of blacklisted species.</returns>
        [HttpGet]
        [Route("api/species/blacklisted")]
        public IEnumerable<string> GetBlacklistedSpecies()
        {
            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();
                    var transaction = myConnection.BeginTransaction();

                    var mySqlCommand = new SqlCommand("Select AssemblyFullName From Species Where BlackListed = 1",
                        myConnection, transaction);
                    var dr = mySqlCommand.ExecuteReader();

                    //TODO: hakridge use a different format other than ArrayList
                    var blackListedSpecies = new ArrayList();
                    while (dr.Read())
                    {
                        blackListedSpecies.Add(dr["AssemblyFullName"]);
                    }

                    if (blackListedSpecies.Count > 0) { return (string[])blackListedSpecies.ToArray(typeof(string)); }
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("GetBlacklistedSpecies", e.ToString());
                return null;
            }

            return null;
        }

        /// <summary>
        ///     Returns a dataset of all species whose population has reached 0 and therefore can be reintroduced.
        /// </summary>
        /// <param name="version">The specific version of the species to get.</param>
        /// <param name="filter">The name of the species to filter or "All" for all available creatures.</param>
        /// <returns>A collection of creatures that meets the version and filter criteria</returns>
        [HttpGet]
        [Route("api/species/extinct")]
        public object GetExtinctSpecies(string version, string filter)
        {
            if (version == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("GetExtinctSpecies", "Suspect: " + GetCurrentRequestIpAddress());
                return null;
            }

            if (filter == null) { filter = string.Empty; }

            version = new Version(version).ToString(3);

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();

                    SqlCommand mySqlCommand;
                    switch (filter)
                    {
                        case "All":
                            mySqlCommand = new SqlCommand("TerrariumGrabExtinctSpecies", myConnection);
                            break;
                        default:
                            mySqlCommand = new SqlCommand("TerrariumGrabExtinctRecentSpecies", myConnection);
                            break;
                    }
                    var adapter = new SqlDataAdapter(mySqlCommand);
                    mySqlCommand.CommandType = CommandType.StoredProcedure;

                    var parmName = mySqlCommand.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    parmName.Value = version;

                    var data = new DataSet();
                    adapter.Fill(data);
                    return data;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("GetExtinctSpecies", e.ToString());
                return null;
            }
        }

        /// <summary>
        ///     Returns a dataset of all species given a specific version and filter criterion.
        /// </summary>
        /// <param name="version">The specific version of the species to get.</param>
        /// <param name="filter">The name of the species to filter or "All" for all available creatures.</param>
        /// <returns>A collection of creatures that meets the version and filter criteria</returns>
        [HttpGet]
        [Route("api/species")]
        public object GetAllSpecies(string version, string filter)
        {
            if (version == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("GetAllSpecies", "Suspect: " + GetCurrentRequestIpAddress());
                return null;
            }

            // Let's verify that this version is even allowed.
            string errorMessage;
            if (VersionChecker.IsVersionDisabled(version, out errorMessage)) { return null; }

            if (filter == null) { filter = string.Empty; }

            version = new Version(version).ToString(3);

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();

                    SqlCommand mySqlCommand;
                    switch (filter)
                    {
                        case "All":
                            mySqlCommand = new SqlCommand("TerrariumGrabAllSpecies", myConnection);
                            break;
                        default:
                            mySqlCommand = new SqlCommand("TerrariumGrabAllRecentSpecies", myConnection);
                            break;
                    }
                    var adapter = new SqlDataAdapter(mySqlCommand);
                    mySqlCommand.CommandType = CommandType.StoredProcedure;

                    var parmVersion = mySqlCommand.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    parmVersion.Value = version;

                    var data = new DataSet();
                    adapter.Fill(data);
                    return data;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("GetAllSpecies", e.ToString());
                return null;
            }
        }

        /// <summary>
        ///     Gets the assembly as a byte array for the species with a given name and version number.
        /// </summary>
        /// <param name="name">The name of the species to get</param>
        /// <param name="version">The version of the species to get</param>
        /// <returns>A byte array of the .NET assembly matching the criterion</returns>
        [HttpGet]
        [Route("api/species/{name}/assembly")]
        public byte[] GetSpeciesAssembly(string name, string version)
        {
            if (name == null || version == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("GetSpeciesAssembly", "Suspect: " + GetCurrentRequestIpAddress());
                return null;
            }

            version = new Version(version).ToString(3);

            try
            {
                var species = TerrariumAssemblyLoader.LoadAssembly(version, name + ".dll");
                return species;
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("GetSpeciesAssembly", e.ToString());
                return null;
            }
        }

        /// <summary>
        ///     Introduces a previously extinct creature back into the EcoSystem and marks it as not extinct.
        /// </summary>
        /// <param name="name">The name of the species to reintroduce</param>
        /// <param name="version">The version of the species to reintroduce</param>
        /// <param name="peerGuid"></param>
        /// <returns>A byte array of the .NET assembly matching the criterion</returns>
        [HttpGet]
        public byte[] ReintroduceSpecies(string name, string version, Guid peerGuid)
        {
            if (name == null || version == null || peerGuid == Guid.Empty)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("ReintroduceSpecies", "Suspect: " + GetCurrentRequestIpAddress());
                return null;
            }

            version = new Version(version).ToString(3);

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();
                    var transaction = myConnection.BeginTransaction();

                    var mySqlCommand = new SqlCommand("TerrariumCheckSpeciesExtinct", myConnection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var parmName = mySqlCommand.Parameters.Add("@Name", SqlDbType.VarChar, 255);
                    parmName.Value = name;

                    var returnValue = mySqlCommand.ExecuteScalar();
                    if (Convert.IsDBNull(returnValue) || ((int)returnValue) == 0)
                    {
                        // the species has already been reintroduced
                        transaction.Rollback();
                        return null;
                    }
                    mySqlCommand = new SqlCommand("TerrariumReintroduceSpecies", myConnection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var parmNode = mySqlCommand.Parameters.Add("@ReintroductionNode", SqlDbType.UniqueIdentifier, 16);
                    parmNode.Value = peerGuid;
                    var parmDateTime = mySqlCommand.Parameters.Add("@LastReintroduction", SqlDbType.DateTime, 8);
                    parmDateTime.Value = DateTime.UtcNow;
                    parmName = mySqlCommand.Parameters.Add("@Name", SqlDbType.VarChar, 255);
                    parmName.Value = name;

                    mySqlCommand.ExecuteNonQuery();

                    var species = TerrariumAssemblyLoader.LoadAssembly(version, name + ".dll");
                    transaction.Commit();
                    return species;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("ReintroduceSpecies", "Species Name: " + name + "\r\n" + e);
                return null;
            }
        }

        /// <summary>
        ///     This function takes a creature assembly, author information,
        ///     and a species name and attempts to insert the creature into the EcoSystem.
        ///     This involves adding the creature to the database and saving the assembly
        ///     on the server so it can later be used for reintroductions.
        ///     All strings are checked for inflammatory words and the insertion is not
        ///     performed if any are found.  In addition the 5 minute rule is checked to
        ///     make sure the user isn't spamming the server.  Only 1 upload is allowed
        ///     per 5 minutes.  An additional constraint of only 30 uploads per day is
        ///     also enforced through the 24 hour rule.
        ///     The creature is then inserted into the database.  If the creature already
        ///     exists the function tells the client the creature is preexisting and the
        ///     insert fails.  If the insert is successful the creature is then saved to disk on the server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="type"></param>
        /// <param name="author"></param>
        /// <param name="email"></param>
        /// <param name="assemblyFullName"></param>
        /// <param name="assemblyCode"></param>
        /// <returns></returns>
        [HttpPost, HttpGet]
        public SpeciesServiceStatus Add(string name, string version, string type, string author, string email,
            string assemblyFullName, byte[] assemblyCode)
        {
            if (name == null || version == null || type == null || author == null || email == null ||
                assemblyFullName == null || assemblyCode == null)
            {
                // Special versioning case, if all parameters are not specified then we return an appropriate error.
                InstallerInfo.WriteEventLog("AddSpecies", "Suspect: " + GetCurrentRequestIpAddress());
                return SpeciesServiceStatus.VersionIncompatible;
            }

            version = new Version(version).ToString(3);

            var nameInappropriate = WordFilter.RunQuickWordFilter(name);
            var authInappropriate = WordFilter.RunQuickWordFilter(author);
            var emailInappropriate = WordFilter.RunQuickWordFilter(email);
            var inappropriate = nameInappropriate | authInappropriate | emailInappropriate;
            var insertComplete = false;

            var allow = !Throttle.Throttled(
                GetCurrentRequestIpAddress(),
                "AddSpecies5MinuteThrottle"
                );


            if (allow)
            {
                allow = !Throttle.Throttled(
                    GetCurrentRequestIpAddress(),
                    "AddSpecies24HourThrottle"
                    );
                if (!allow) { return SpeciesServiceStatus.TwentyFourHourThrottle; }
            }
            else
            {
                return SpeciesServiceStatus.FiveMinuteThrottle;
            }

            try
            {
                using (var myConnection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    myConnection.Open();
                    var transaction = myConnection.BeginTransaction();

                    var mySqlCommand = new SqlCommand("TerrariumInsertSpecies", myConnection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var parmName = mySqlCommand.Parameters.Add("@Name", SqlDbType.VarChar, 255);
                    parmName.Value = name;
                    var parmVersion = mySqlCommand.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    parmVersion.Value = version;
                    var parmType = mySqlCommand.Parameters.Add("@Type", SqlDbType.VarChar, 50);
                    parmType.Value = type;

                    var parmAuthor = mySqlCommand.Parameters.Add("@Author", SqlDbType.VarChar, 255);
                    parmAuthor.Value = author;
                    var parmAuthorEmail = mySqlCommand.Parameters.Add("@AuthorEmail", SqlDbType.VarChar, 255);
                    parmAuthorEmail.Value = email;

                    var parmExtinct = mySqlCommand.Parameters.Add("@Extinct", SqlDbType.TinyInt, 1);
                    parmExtinct.Value = 0;
                    var parmDateAdded = mySqlCommand.Parameters.Add("@DateAdded", SqlDbType.DateTime, 8);
                    parmDateAdded.Value = DateTime.Now;
                    var parmAssembly = mySqlCommand.Parameters.Add("@AssemblyFullName", SqlDbType.Text, 16);
                    parmAssembly.Value = assemblyFullName;

                    var parmBlackListed = mySqlCommand.Parameters.Add("@BlackListed", SqlDbType.Bit, 1);
                    parmBlackListed.Value = inappropriate;

                    try
                    {
                        mySqlCommand.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        // 2627 is Primary key violation
                        if (e.Number == 2627) { return SpeciesServiceStatus.AlreadyExists; }
                        throw;
                    }

                    var introductionWait = ServerSettings.IntroductionWait;

                    Throttle.AddThrottle(
                        GetCurrentRequestIpAddress(),
                        "AddSpecies5MinuteThrottle",
                        1,
                        DateTime.Now.AddMinutes(introductionWait)
                        );

                    var introductionDailyLimit = ServerSettings.IntroductionDailyLimit;

                    Throttle.AddThrottle(
                        GetCurrentRequestIpAddress(),
                        "AddSpecies24HourThrottle",
                        introductionDailyLimit,
                        DateTime.Now.AddHours(24)
                        );
                    insertComplete = true;
                    TerrariumAssemblyLoader.SaveAssembly(assemblyCode, version, name + ".dll");
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("AddSpecies", e.ToString());

                if (insertComplete) { TerrariumAssemblyLoader.RemoveAssembly(version, name); }

                return SpeciesServiceStatus.ServerDown;
            }

            //hakridge: Inappropriate is the combination of the below three if statements. Can just simplify the final return, but being explicit might be expected
            if (inappropriate)
            {
                if (nameInappropriate) { return SpeciesServiceStatus.PoliCheckSpeciesNameFailure; }
                if (authInappropriate) { return SpeciesServiceStatus.PoliCheckAuthorNameFailure; }
                if (emailInappropriate) { return SpeciesServiceStatus.PoliCheckEmailFailure; }

                return SpeciesServiceStatus.AlreadyExists;
            }

            return SpeciesServiceStatus.Success;
        }
    }
}