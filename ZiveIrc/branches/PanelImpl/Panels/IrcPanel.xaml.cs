using System;
using System.Windows;
using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public interface IIrcPanel {

        void FocusTextInputArea( );

        event RoutedEventHandler GotFocus;

    }

    public partial class IrcPanel: UserControl, IIrcPanel {

        public IrcPanel( ) {
            InitializeComponent( );
        }

        public void FocusTextInputArea( ) {
            TextInputArea.Focus( );
        }

    }

}
