using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ArmoSystems.ArmoGet.VariablesManager.Classes
{
    internal static class FilesHelper
    {
        public const string DefaultBranchName = "default";
        public const string VariablesFolderTimex = @"D:\Projects\Timex\Build\Variables\Computers";
        public const string VariablesFolderSmartec = @"D:\Projects\smartecdevice\Build\Variables";
        private const string CyrillicServerHostsServiceAddress = "127.0.0.1 СерверСервис.таймекс.рф";
        public static readonly string CustomizedTextFile = $"{Environment.MachineName}.txt";

        private static string HostsFilePath => $@"{Environment.GetEnvironmentVariable( "windir" )}\System32\drivers\etc\hosts";

        public static void AppendHostsIfNotExists()
        {
            if ( !File.ReadLines( HostsFilePath, Encoding.Default ).Any( line => line.Equals( CyrillicServerHostsServiceAddress ) ) )
                File.AppendAllText( HostsFilePath, $"\n{CyrillicServerHostsServiceAddress}", Encoding.Default );
        }
    }
}