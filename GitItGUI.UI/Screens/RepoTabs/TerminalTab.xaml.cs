using GitItGUI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitItGUI.UI.Screens.RepoTabs
{
    /// <summary>
    /// Interaction logic for TerminalTab.xaml
    /// </summary>
    public partial class TerminalTab : UserControl
    {
		private bool refreshPending;

        public TerminalTab()
        {
            InitializeComponent();
			DebugLog.WriteCallback += DebugLog_WriteCallback;
			cmdTextBox.KeyDown += CmdTextBox_KeyDown;
        }

		public void ScrollToEnd()
		{
			terminalTextBox.ScrollToEnd();
		}

		public void CheckRefreshPending()
		{
			if (!refreshPending) return;
			refreshPending = false;
			RepoScreen.singleton.Refresh();
		}

		public void Refresh()
		{
			const int maxLength = 60000;
			string text = terminalTextBox.Text;
			if (text.Length > maxLength) terminalTextBox.Text = text.Remove(0, text.Length - maxLength);
			ScrollToEnd();
		}
		
		private void DebugLog_WriteCallback(string value)
		{
			if (Dispatcher.CheckAccess())
			{
				terminalTextBox.AppendText(value + Environment.NewLine);
			}
			else
			{
				Dispatcher.InvokeAsync(delegate()
				{
					terminalTextBox.AppendText(value + Environment.NewLine);
				});
			}
		}

		private void CmdTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return) runCmdButton_Click(null, null);
		}

		private void runCmdButton_Click(object sender, RoutedEventArgs e)
		{
			refreshPending = true;
			string cmd = cmdTextBox.Text;
			cmdTextBox.Text = string.Empty;
			RepoScreen.singleton.repoManager.dispatcher.InvokeAsync(delegate()
			{
				RepoScreen.singleton.repoManager.repository.RunGenericCmd(cmd);
				Dispatcher.InvokeAsync(delegate()
				{
					ScrollToEnd();
				});
			});
		}
	}
}
