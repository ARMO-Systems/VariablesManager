﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArmoSystems.ArmoGet.VariablesManager.Classes;

namespace ArmoSystems.ArmoGet.VariablesManager
{
    internal static class VariablesManager
    {
        public const EnvironmentVariableTarget VariableTarget = EnvironmentVariableTarget.User;

        public static void UpdateVariables()
        {
            if ( !FilesHelper.FileExists( FilesHelper.DefaultTextFile ) )
                throw new Exception( String.Format( "Не найден файл {0}", FilesHelper.DefaultTextFile ) );

            var variablesForSet =
                GetFileVariables( FilesHelper.DefaultTextFile ).
                    Concat( GetFileVariables( FilesHelper.CustomizedTextFile ) ).
                    Concat( GetCalculateVariables() ).
                    GroupBy( variable => variable.Name ).
                    Select( group => group.OrderBy( variable => variable.Priority ).Last() ).
                    ToList();

            RemoveUnusedVariables( variablesForSet );
            SetVariables( variablesForSet );

            FilesHelper.AppendHostsIfNotExists();
        }

        private static void SetVariables( List< Variable > variablesForSet )
        {
            while ( variablesForSet.Any( item => item.Value.Contains( "%Timex" ) ) )
            {
                variablesForSet.Where( item => item.Value.Contains( "%Timex" ) ).ToList().ForEach( item =>
                                                                                                   {
                                                                                                       var match = Regex.Match( item.Value, ".*(%Timex.*%).*" );
                                                                                                       while ( match.Success )
                                                                                                       {
                                                                                                           var variableInside = match.Groups[ 1 ].Value;
                                                                                                           item.Value = item.Value.Replace( variableInside,
                                                                                                               variablesForSet.First( item1 => item1.Name == variableInside.Substring( 1, variableInside.Length - 2 ) ).Value );
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
                ForEach( name => Environment.SetEnvironmentVariable( name, String.Empty, VariableTarget ) );
        }

        private static int GetVariablePriority( string variableFileName, string variableBranchName )
        {
            return ( variableFileName == FilesHelper.DefaultTextFile ? 0 : 2 ) + ( variableBranchName == FilesHelper.DefaultBranchName ? 1 : 2 );
        }

        private static IEnumerable< Variable > GetFileVariables( string fileName )
        {
            if ( !FilesHelper.FileExists( fileName ) )
                return Enumerable.Empty< Variable >();

            var fileText = FilesHelper.ReadFileText( fileName );
            var variablesBranches = new Regex( @"<([^\r>]+)>([^<]+)" ).Matches( fileText ).Cast< Match >().ToList();
            if ( fileText != variablesBranches.Select( group => group.Groups[ 0 ].Value ).Aggregate( String.Empty, ( str1, str2 ) => str1 + str2 ) )
                throw new Exception( String.Format( "Неверный формат файла {0}", fileName ) );

            return
                variablesBranches.Select( branch => new { Name = branch.Groups[ 1 ].Value, Variables = branch.Groups[ 2 ].Value } ).
                    Where( branch => branch.Name == FilesHelper.DefaultBranchName || branch.Name == FilesHelper.CurrentBranchName ).
                    SelectMany(
                        branch => branch.Variables.Split( new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries ).Select( item => new { Priority = GetVariablePriority( fileName, branch.Name ), Values = item.Split( '=' ) } ) ).
                    Select( item => new Variable( item.Values[ 0 ], item.Values[ 1 ], item.Priority ) );
        }

        private static Variable CreateCalculateVariable( string name, string value )
        {
            const string calculateVariablePrefix = "TimexCalculate";
            const int calculateVariablePriority = 5;
            return new Variable( String.Format( "{0}{1}", calculateVariablePrefix, name ), value, calculateVariablePriority );
        }

        private static IEnumerable< Variable > GetCalculateVariables()
        {
            return new List< Variable >
            {
                CreateCalculateVariable( "CommonMSBuildPath", FilesHelper.PathToMSBuild ),
                CreateCalculateVariable( "CommonPlatformBit", String.Format( "x{0}", Environment.Is64BitOperatingSystem ? "64" : "86" ) )
            };
        }
    }
}