using System;
using System.Data;
using System.Data.SqlClient;
using Terrarium.Server.Helpers;

namespace Terrarium.Server.Services
{
    public static class VersionChecker
    {
        public static bool IsVersionDisabled(string version, out string errorMessage)
        {
            try
            {
                using (var connection = new SqlConnection(ServerSettings.SpeciesDsn))
                {
                    connection.Open();

                    var command = new SqlCommand("TerrariumIsVersionDisabled", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var fullVersion = new Version(version).ToString(4);
                    version = new Version(version).ToString(3);

                    var versionParameter = command.Parameters.Add("@Version", SqlDbType.VarChar, 255);
                    versionParameter.Value = version;
                    var fullVersionParameter = command.Parameters.Add("@FullVersion", SqlDbType.VarChar, 255);
                    fullVersionParameter.Value = fullVersion;

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var disabled = Convert.ToBoolean(reader["Disabled"]);
                        errorMessage = disabled ? Convert.ToString(reader["Message"]) : "";
                        return disabled;
                    }
                    errorMessage = "";
                    return true;
                }
            }
            catch (Exception e)
            {
                InstallerInfo.WriteEventLog("CheckVersion", e.ToString());
                errorMessage = "";
                return true;
            }
        }
    }
}