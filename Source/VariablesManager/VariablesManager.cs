using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArmoSystems.ArmoGet.VariablesManager.Classes;

namespace ArmoSystems.ArmoGet.VariablesManager
{
    internal static class VariablesManager
    {
        public const EnvironmentVariableTarget VariableTarget = EnvironmentVariableTarget.User;
        private const string LocalvarsFileName = "LocalVars.txt";
        private static readonly VariablesFile DefaultLocalFile = new VariablesFile( @"D:\Temp", LocalvarsFileName );

        private static readonly VariablesFile DefaultTimexFile = new VariablesFile( FilesHelper.VariablesFolderTimex, VariablesFile.DefaultTextFile );
        private static readonly VariablesFile CustomTimexFile = new VariablesFile( FilesHelper.VariablesFolderTimex, VariablesFile.CustomizedTextFile );

        private static readonly VariablesFile DefaultSmartecFile = new VariablesFile( FilesHelper.VariablesFolderSmartec, VariablesFile.DefaultTextFile );
        private static readonly VariablesFile CustomSmartecFile = new VariablesFile( FilesHelper.VariablesFolderSmartec, VariablesFile.CustomizedTextFile );

        public static void UpdateVariables()
        {
            if ( !DefaultTimexFile.FileExists() && !DefaultSmartecFile.FileExists() )
                throw new Exception( $"Не найден файл {DefaultTimexFile.FileName}" );

            var variablesFiles = new[] { DefaultLocalFile, DefaultTimexFile, DefaultSmartecFile }.Concat( CustomTimexFile.GetVariantsCustomFilePaths() ).Concat( CustomSmartecFile.GetVariantsCustomFilePaths() );
            var variablesForSet = variablesFiles.SelectMany( GetFileVariables ).
                                                 Concat( GetCalculateVariables() ).
                                                 GroupBy( variable => variable.Name ).
                                                 Select( group => group.OrderBy( variable => variable.Priority ).Last() ).
                                                 ToList();
            variablesForSet.AddRange( GetCalculateVariables( variablesForSet ) );
            RemoveUnusedVariables( variablesForSet );
            SetVariables( variablesForSet );

            FilesHelper.AppendHostsIfNotExists();
        }

        private static IEnumerable< Variable > GetCalculateVariables( List< Variable > variablesForSet )
        {
            return new List< Variable >
            {
                CreateCalculateVariable( "CommonMicrosoftSDKTools",
                    GetValue( variablesForSet, "TimexIsVS2015", false )
                        ? @"c:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools"
                        : @"c:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\Bin\NETFX 4.0 Tools" ),
                CreateCalculateVariable( "CommonTextTemplating", @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE" ),
                CreateCalculateVariable( "CommonMSBuildPath", @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" )
            };
        }

        private static bool GetValue( IEnumerable< Variable > variablesForSet, string name, bool defaultValue )
        {
            var val = variablesForSet.FirstOrDefault( item => item.Name == name );
            return val == null ? defaultValue : Convert.ToBoolean( val.Value );
        }

        private static void SetVariables( List< Variable > variablesForSet )
        {
            while ( variablesForSet.Any( item => item.Value.Contains( "%Timex" ) ) )
            {
                variablesForSet.Where( item => item.Value.Contains( "%Timex" ) ).
                                ToList().
                                ForEach( item =>
                                         {
                                             var match = Regex.Match( item.Value, ".*(%Timex.*%).*" );
                                             while ( match.Success )
                                             {
                                                 var variableInside = match.Groups[ 1 ].Value;
                                                 item.Value = item.Value.Replace( variableInside, variablesForSet.First( item1 => item1.Name == variableInside.Substring( 1, variableInside.Length - 2 ) ).Value );
                                                 match = match.NextMatch();
                                             }
                                         } );
            }

            variablesForSet.ForEach( item => item.SetEnviromentVariable() );
        }

        private static void RemoveUnusedVariables( IEnumerable< Variable > variablesForSet )
        {
            Environment.GetEnvironmentVariables( VariableTarget ).
                        Keys.Cast< string >().
                        Where( item => item.StartsWith( "Timex", StringComparison.InvariantCulture ) && variablesForSet.All( v => v.Name != item ) ).
                        ToList().
                        ForEach( name => Environment.SetEnvironmentVariable( name, string.Empty, VariableTarget ) );
        }

        private static int GetVariablePriority( VariablesFile variableFileName, string variableBranchName )
        {
            switch ( variableFileName.FileName )
            {
                case LocalvarsFileName:
                    return 1000;
                case VariablesFile.DefaultTextFile:
                    return 0;
                default:
                    return variableFileName.FileName.Length + ( variableBranchName == FilesHelper.DefaultBranchName ? 1 : 2 );
            }
        }

        private static IEnumerable< Variable > GetFileVariables( VariablesFile variablesFile )
        {
            if ( !variablesFile.FileExists() )
                return Enumerable.Empty< Variable >();

            var fileText = variablesFile.ReadFileText();
            var variablesBranches = new Regex( @"<([^\r>]+)>([^<]+)" ).Matches( fileText ).Cast< Match >().ToList();
            if ( fileText != variablesBranches.Select( group => group.Groups[ 0 ].Value ).Aggregate( string.Empty, ( str1, str2 ) => str1 + str2 ) )
                throw new Exception( $"Неверный формат файла {variablesFile}" );

            return variablesBranches.Select( branch => new { Name = branch.Groups[ 1 ].Value, Variables = branch.Groups[ 2 ].Value } ).
                                     Where( branch => branch.Name == FilesHelper.DefaultBranchName || branch.Name == variablesFile.CurrentBranchName ).
                                     SelectMany( branch => branch.Variables.Split( new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries ).
                                                                  Select( item => new { Priority = GetVariablePriority( variablesFile, branch.Name ), Values = item.Split( '=' ) } ) ).
                                     Select( item => new Variable( item.Values[ 0 ], item.Values[ 1 ], item.Priority ) );
        }

        private static Variable CreateCalculateVariable( string name, string value )
        {
            const string calculateVariablePrefix = "TimexCalculate";
            const int calculateVariablePriority = 5;
            return new Variable( $"{calculateVariablePrefix}{name}", value, calculateVariablePriority );
        }

        private static IEnumerable< Variable > GetCalculateVariables()
        {
            return new List< Variable > { CreateCalculateVariable( "CommonPlatformBit", $"x{( Environment.Is64BitOperatingSystem ? "64" : "86" )}" ) };
        }
    }
}