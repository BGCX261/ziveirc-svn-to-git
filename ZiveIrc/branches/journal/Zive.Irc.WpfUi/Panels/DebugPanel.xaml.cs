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

        // Properties

        public bool DebuggingOutputEnabled {
            get {
#if DEBUG
                return Trace.Listeners.Contains( _traceListener );
#else
                return false;
#endif
            }
            set {
#if DEBUG
                if ( value ) {
                    DebugX.Print( "ConsolePanel.DebuggingOutputEnabled.set: enabling console panel trace listener" );
                    Trace.Listeners.Add( _traceListener );
                } else {
                    DebugX.Print( "ConsolePanel.DebuggingOutputEnabled.set: disabling console panel trace listener" );
                    Trace.Listeners.Remove( _traceListener );
                }
#endif
            }
        }

        // Events

        // Constructors

        public DebugPanel( ) {
            InitializeComponent( );

            _scrollActions = new Dictionary<ScrollAction, Action> {
                { ScrollAction.PageUp,   ScrollViewer.PageUp         },
                { ScrollAction.PageDown, ScrollViewer.PageDown       },
                { ScrollAction.LineUp,   ScrollViewer.LineUp         },
                { ScrollAction.LineDown, ScrollViewer.LineDown       },
                { ScrollAction.Top,      ScrollViewer.ScrollToTop    },
                { ScrollAction.Bottom,   ScrollViewer.ScrollToBottom },
            };

#if DEBUG
            _scrollbackDebugManager = new ScrollbackDebugManager {
                FlowDocument = ScrollbackViewer.Document,
            };

            _traceListener = new DebugPanelTraceListener {
                Name = "DebugPanelTraceListener",
                ScrollbackDebugManager = _scrollbackDebugManager,
            };
#endif
        }

        // Methods

        public void FocusInputBar( ) {
            InputBar.Focus( );
        }

        //
        // Implementation
        //

        protected readonly Dictionary<ScrollAction, Action> _scrollActions;

#if DEBUG
        protected ScrollbackDebugManager _scrollbackDebugManager;
        protected DebugPanelTraceListener _traceListener;
#endif

        // Properties

        // Events

        protected void RoutedEvent_GotFocus( object sender, RoutedEventArgs ev ) {
            InputBar.Focus( );
        }

        protected void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs ev ) {
            if ( ev.Source.GetType( ) == typeof( ScrollViewer ) ) {
                return;
            }

            // Calculate scrollbar situation before the scroll change
            double oldVh = ev.ViewportHeight - ev.ViewportHeightChange;
            double oldVo = ev.VerticalOffset - ev.VerticalChange;
            double oldEh = ev.ExtentHeight - ev.ExtentHeightChange;

            var oldNeeded = ( oldEh >= oldVh );
            var oldAtBottom = Math.Abs( ( oldVh + oldVo ) - oldEh ) < 0.01;

            // Calculate scrollbar situation after the scroll change
            double newVh = ev.ViewportHeight;
            double newVo = ev.VerticalOffset;
            double newEh = ev.ExtentHeight;

            var newNeeded = ( newEh >= newVh );
            var newAtBottom = Math.Abs( ( newVh + newVo ) - newEh ) < 0.01;

            if ( ( !oldNeeded && newNeeded ) || ( oldAtBottom && !newAtBottom ) ) {
                Dispatcher.BeginInvoke( (Action) ScrollViewer.ScrollToBottom );
            }
        }

        protected void InputBar_LineInput( object sender, LineInputEventArgs ev ) {
            Debug.Print( "DebugPanel.InputBar_LineInput: input line is '{0}'", ev.Line );
        }

        protected void InputBar_RelayedScroll( object sender, RelayedScrollEventArgs ev ) {
            _scrollActions[ ev.ScrollAction ]( );
        }

        // Constructors

        // Methods

    }

}
