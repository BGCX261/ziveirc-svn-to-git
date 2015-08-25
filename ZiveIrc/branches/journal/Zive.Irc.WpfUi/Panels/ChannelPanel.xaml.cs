using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public partial class ChannelPanel: UserControl, IIrcPanel {

        public ChannelPanel( ) {
            InitializeComponent( );
        }

        public void FocusInputBar( ) {
            TextInputArea.Focus( );
        }

    }

}
