using System;
using System.IO;
using System.Windows.Forms;
using ArmoSystems.ArmoGet.VariablesManager.Classes;
using ArmoSystems.ArmoGet.VariablesManager.Properties;

namespace ArmoSystems.ArmoGet.VariablesManager
{
    internal sealed partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            fileSystemWatcher.Path = FilesHelper.VariablesFolderTimex;
            fileSystemWatcherSmartec.Path = FilesHelper.VariablesFolderSmartec;
            UpdateVariables();
        }

        private void UpdateVariables()
        {
            try
            {
                VariablesManager.UpdateVariables();
                UpdateNotifyIcon( "Переменные среды и файл hosts обновлены", ToolTipIcon.Info );
            }
            catch ( Exception ex )
            {
                UpdateNotifyIcon( ex.Message.Length > 64 ? ex.Message.Substring( 0, 63 ) : ex.Message, ToolTipIcon.Error );
            }
        }

        private void UpdateNotifyIcon( string ballonMessage, ToolTipIcon tipIcon )
        {
            trayIcon.ShowBalloonTip( 3000, string.Empty, ballonMessage, tipIcon );
            trayIcon.Icon = tipIcon == ToolTipIcon.Error ? Resources.Error : Resources.Eyeball;
            trayIcon.Text = tipIcon == ToolTipIcon.Error ? ballonMessage : Program.ProgramName;
        }

        private void fileSystemWatcher_Changed( object sender, FileSystemEventArgs e )
        {
            try
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                var changedFileName = e.Name.ToLower();
                if ( changedFileName == FilesHelper.CustomizedTextFile.ToLower() || changedFileName == VariablesFile.DefaultTextFile.ToLower() )
                    UpdateVariables();
            }
            catch ( Exception ex )
            {
                UpdateNotifyIcon( ex.Message, ToolTipIcon.Error );
            }
            finally
            {
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void menuExit_Click( object sender, EventArgs e )
        {
            Close();
        }

        private void menuSetVariables_Click( object sender, EventArgs e )
        {
            UpdateVariables();
        }
    }
}