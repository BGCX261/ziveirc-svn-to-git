using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    [AttributeUsage( AttributeTargets.All )]
    internal class MoveMe: Attribute {

    }

    public partial class ConsolePanel: UserControl, IIrcPanel {

        //
        // Interface
        //

        internal PanelImpl PanelImpl;

        public ScrollbackManager ScrollbackManager { get { return PanelImpl.ScrollbackManager; } }

        public InputProcessor InputProcessor {
            get { return PanelImpl.InputProcessor; }
            set { PanelImpl.InputProcessor = value; }
        }

        public ConsolePanel( ) {
            InitializeComponent( );

            PanelImpl = new PanelImpl {
                IrcPanel = this,
                ScrollViewer = ScrollViewer,
                ScrollbackViewer = ScrollbackViewer,
                InputBar = InputBar,
                Dispatcher = Dispatcher,
            };
            PanelImpl.InitializeComponent( );

            ScrollbackManager.AddLine( "Welcome to ZiveIRC." );
        }

        public void FocusTextInputArea( ) {
            PanelImpl.FocusTextInputArea( );
        }

    }

}
