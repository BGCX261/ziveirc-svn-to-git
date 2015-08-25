using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Zive.Irc.WpfUi {

    public partial class AboutDialog: Window {

        public AboutDialog( ) {
            InitializeComponent( );

            VersionText.Text = Assembly.GetEntryAssembly( ).GetName( ).Version.ToString( );
        }

        private void Close_OnCanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = true;
        }

        private void Close_OnExecuted( object sender, ExecutedRoutedEventArgs ev ) {
            DialogResult = true;
            Close( );
        }

    }

}
