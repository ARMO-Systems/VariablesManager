using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ArmoSystems.ArmoGet.VariablesManager.Classes
{
    internal sealed class VariablesFile
    {
        public const string DefaultTextFile = "default-PC.txt";
        private const int AttemptsForReadFile = 100;
        public static readonly string CustomizedTextFile;
        private static readonly List< string > CustomFilesVariants;

        static VariablesFile()
        {
            CustomizedTextFile = $"{Environment.MachineName}.txt";
            var customFileName = Path.GetFileNameWithoutExtension( CustomizedTextFile );
            CustomFilesVariants = Enumerable.Range( 1, customFileName.Length ).Select( item => customFileName.Substring( 0, item ) + ".txt" ).ToList();
        }

        public VariablesFile( string folder, string fileName )
        {
            Folder = folder;
            FileName = fileName;
        }

        public string FileName { get; }

        public string Folder { get; }

        public string CurrentBranchName
        {
            get
            {
                var gitPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ProgramFiles ), "Git", "bin", "git.exe" );
                var process =
                    Process.Start( new ProcessStartInfo( gitPath, "rev-parse --abbrev-ref HEAD" )
                    {
                        WorkingDirectory = Folder,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    } );
                process.WaitForExit();
                return process.StandardOutput.ReadLine();
            }
        }

        public string ReadFileText()
        {
            var counter = 0;
            while ( true )
            {
                try
                {
                    return File.ReadAllText( GetVariablesFilePath() );
                }
                catch ( IOException )
                {
                    if ( counter++ > AttemptsForReadFile )
                        throw;
                }
            }
        }

        public string GetVariablesFilePath()
        {
            return GetVariablesFilePath( FileName );
        }

        private string GetVariablesFilePath( string fileName )
        {
            return Path.Combine( Folder, fileName );
        }

        public bool FileExists()
        {
            var counter = 0;
            while ( counter++ < AttemptsForReadFile )
            {
                if ( File.Exists( GetVariablesFilePath() ) )
                    return true;
            }
            return false;
        }

        public IEnumerable< VariablesFile > GetVariantsCustomFilePaths()
        {
            return CustomFilesVariants.Select( item => new VariablesFile( Folder, item ) );
        }
    }
}