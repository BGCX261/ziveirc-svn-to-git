using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Zive.Irc.Utility;

namespace Zive.Irc.WpfUi {

    public partial class DebugPanel: UserControl, IIrcPanel {

        //
        // Interface
        //

        internal PanelImpl PanelImpl;

        public ScrollbackManager ScrollbackManager { get { return PanelImpl.ScrollbackManager; } }
       
        public InputProcessor InputProcessor {
            get { return PanelImpl.InputProcessor; }
            set { PanelImpl.InputProcessor = value; }
        }

        public bool DebuggingOutputEnabled {
            get {
#if DEBUG
                return Trace.Listeners.Contains( TraceListener );
#else
                return false;
#endif
            }
            set {
#if DEBUG
                if ( value ) {
                    DebugX.Print( "ConsolePanel.DebuggingOutputEnabled.set: enabling console panel trace listener" );
                    Trace.Listeners.Add( TraceListener );
                } else {
                    DebugX.Print( "ConsolePanel.DebuggingOutputEnabled.set: disabling console panel trace listener" );
                    Trace.Listeners.Remove( TraceListener );
                }
#endif
            }
        }

        // Events

        // Constructors

        public DebugPanel( ) {
            InitializeComponent( );

            PanelImpl = new PanelImpl {
                IrcPanel = this,
                ScrollViewer = ScrollViewer,
                ScrollbackViewer = ScrollbackViewer,
                InputBar = InputBar,
                Dispatcher = Dispatcher,
            };
            PanelImpl.InitializeComponent( );

#if DEBUG
            ScrollbackDebugManager = new ScrollbackDebugManager {
                FlowDocument = ScrollbackViewer.Document,
            };

            TraceListener = new DebugPanelTraceListener {
                Name = "DebugPanelTraceListener",
                ScrollbackDebugManager = ScrollbackDebugManager,
            };
#endif
        }

        public void FocusTextInputArea( ) {
            PanelImpl.FocusTextInputArea( );
        }

        protected DebugPanelTraceListener TraceListener;
        protected ScrollbackDebugManager ScrollbackDebugManager;

    }

}
