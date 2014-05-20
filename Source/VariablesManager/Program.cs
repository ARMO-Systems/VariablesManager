using System;
using Microsoft.VisualBasic.ApplicationServices;

namespace ArmoSystems.ArmoGet.VariablesManager
{
    internal static class Program
    {
        public const string ProgramName = "Variables Manager";

        [STAThread]
        public static void Main()
        {
            if ( Environment.GetCommandLineArgs().Length == 1 )
                new SingleInstanceController().Run( Environment.GetCommandLineArgs() );
            else
                VariablesManager.UpdateVariables();
        }

        private sealed class SingleInstanceController : WindowsFormsApplicationBase
        {
            public SingleInstanceController()
            {
                IsSingleInstance = true;
            }

            protected override void OnCreateMainForm()
            {
                MainForm = new MainForm();
            }
        }
    }
}