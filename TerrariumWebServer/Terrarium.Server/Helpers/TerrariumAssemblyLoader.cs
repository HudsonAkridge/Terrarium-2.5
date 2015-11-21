using System;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace Terrarium.Server.Helpers
{
    public static class TerrariumAssemblyLoader
    {
        /*
        Method:     LoadAssembly
        Purpose:    Grabs an assembly off the disk and returns it as a
        byte array.
    */

        public static byte[] LoadAssembly(string version, string assemblyFileName)
        {
            var assemblyRoot = ServerSettings.AssemblyPath;
            version = new Version(version).ToString(3);

            var permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new[] { assemblyRoot + "\\" + version });
            byte[] bytes;
            try
            {
                permission.PermitOnly();

                using (var sourceStream = File.OpenRead(assemblyRoot + "\\" + version + "\\" + assemblyFileName))
                {
                    bytes = new byte[sourceStream.Length];
                    sourceStream.Read(bytes, 0, (int)sourceStream.Length);
                }
            }
            finally
            {
                CodeAccessPermission.RevertPermitOnly();
            }

            return bytes;
        }

        /*
                Method:     RemoveAssembly
                Purpose:    Attempts to delete an assembly from the servers disk
                cache.
            */

        public static void RemoveAssembly(string version, string assemblyFileName)
        {
            var assemblyRoot = ServerSettings.AssemblyPath;
            version = new Version(version).ToString(3);

            var path = new DirectoryInfo(assemblyRoot + "\\" + version);
            if (!path.Exists)
                return;

            var permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new[] { assemblyRoot + "\\" + version });
            try
            {
                permission.PermitOnly();
                if (File.Exists(assemblyRoot + "\\" + version + "\\" + assemblyFileName))
                    File.Delete(assemblyRoot + "\\" + version + "\\" + assemblyFileName);
            }
            finally
            {
                CodeAccessPermission.RevertPermitOnly();
            }
        }

        /*
                Method:     SaveAssembly
                Purpose:    Attemps to save a byte array as an assembly on the
                servers disk cache.
            */

        public static void SaveAssembly(byte[] assemblyCode, string version, string assemblyFileName)
        {
            var assemblyRoot = ServerSettings.AssemblyPath;
            version = new Version(version).ToString(3);

            var path = new DirectoryInfo(assemblyRoot + "\\" + version);
            if (!path.Exists)
                path.Create();

            var permission = new FileIOPermission(FileIOPermissionAccess.AllAccess,
                new[] { assemblyRoot + "\\" + version });
            try
            {
                permission.PermitOnly();

                // Use CreateNew to create so we get an exception if the file already exists -- it never should
                using (
                    var targetStream = File.Open(assemblyRoot + "\\" + version + "\\" + assemblyFileName, FileMode.CreateNew))
                {
                    try
                    {
                        targetStream.Write(assemblyCode, 0, assemblyCode.Length);
                        targetStream.Close();
                    }
                    catch
                    {
                        targetStream.Close();

                        // If something happens, delete the file so we don't have
                        // a corrupted file hanging around
                        File.Delete(assemblyRoot + "\\" + version + "\\" + assemblyFileName);

                        throw;
                    }
                }
            }
            finally
            {
                CodeAccessPermission.RevertPermitOnly();
            }
        }
    }
}