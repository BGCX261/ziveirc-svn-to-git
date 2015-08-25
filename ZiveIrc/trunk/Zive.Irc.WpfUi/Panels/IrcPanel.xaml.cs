using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public interface IIrcPanel {

        void FocusInputBar( );

    }

    public partial class IrcPanel: UserControl, IIrcPanel {

        public IrcPanel( ) {
            InitializeComponent( );
        }

        public void FocusInputBar( ) {
            TextInputArea.Focus( );
        }

    }

}
