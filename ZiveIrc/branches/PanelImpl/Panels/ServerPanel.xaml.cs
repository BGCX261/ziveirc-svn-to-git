using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public partial class ServerPanel: UserControl, IIrcPanel {

        public ServerPanel( ) {
            InitializeComponent( );
        }

        public void FocusTextInputArea( ) {
            TextInputArea.Focus( );
        }

    }

}
