using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public partial class QueryPanel: UserControl, IIrcPanel {

        public QueryPanel( ) {
            InitializeComponent( );
        }

        public void FocusInputBar( ) {
            TextInputArea.Focus( );
        }

    }

}
