using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ArmoSystems.ArmoGet.VariablesManager.Classes
{
    internal static class FilesHelper
    {
        private const int AttemptsForReadFile = 100;
        private const string TimexFolderPath = @"D:\Projects\Timex";
        public const string DefaultTextFile = "default-PC.txt";
        public const string DefaultBranchName = "default";
        public const string VariablesFolder = @"D:\Projects\Timex\Build\Variables\Computers";
        private const string CyrillicServerHostsServiceAddress = "127.0.0.1 СерверСервис.таймекс.рф";
        public static readonly string CustomizedTextFile = String.Format( "{0}.txt", Environment.MachineName );

        private static string HostsFilePath
        {
            get { return string.Format( @"{0}\System32\drivers\etc\hosts", Environment.GetEnvironmentVariable( "windir" ) ); }
        }

        public static string PathToMSBuild
        {
            get
            {
                var pathWithInstalled64BitMsBuild = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ProgramFiles ), @"MSBuild\12.0\bin\AMD64" );
                return Directory.Exists( pathWithInstalled64BitMsBuild ) ? pathWithInstalled64BitMsBuild : Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ProgramFilesX86 ), @"MSBuild\12.0\bin" );
            }
        }

        public static string CurrentBranchName
        {
            get
            {
                var gitPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ProgramFiles ), "Git", "bin", "git.exe" );
                var process =
                    Process.Start( new ProcessStartInfo( gitPath, "rev-parse --abbrev-ref HEAD" )
                    {
                        WorkingDirectory = TimexFolderPath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    } );
                process.WaitForExit();
                return process.StandardOutput.ReadLine();
            }
        }

        public static string ReadFileText( string fileName )
        {
            var counter = 0;
            while ( true )
            {
                try
                {
                    return File.ReadAllText( GetVariablesFilePath( fileName ) );
                }
                catch ( IOException )
                {
                    if ( counter++ > AttemptsForReadFile )
                        throw;
                }
            }
        }

        public static string GetVariablesFilePath( string fileName )
        {
            return Path.Combine( VariablesFolder, fileName );
        }

        public static bool FileExists( string monitoringFileName )
        {
            return File.Exists( GetVariablesFilePath( monitoringFileName ) );
        }

        public static void AppendHostsIfNotExists()
        {
            if ( !File.ReadLines( HostsFilePath, Encoding.Default ).Any( line => line.Equals( CyrillicServerHostsServiceAddress ) ) )
                File.AppendAllText( HostsFilePath, string.Format( "\n{0}", CyrillicServerHostsServiceAddress ), Encoding.Default );
        }
    }
}