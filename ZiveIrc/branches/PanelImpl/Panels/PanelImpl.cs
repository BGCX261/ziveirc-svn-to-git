using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Zive.Irc.WpfUi {

    internal class PanelImpl {

        //
        // Interface
        //

        // Fields

        public IIrcPanel IrcPanel;
        public ScrollViewer ScrollViewer;
        public FlowDocumentScrollViewer ScrollbackViewer;
        public InputBar InputBar;
        public Dispatcher Dispatcher;

        public InputProcessor InputProcessor;
        public ScrollbackManager ScrollbackManager;

        // Methods

        public void InitializeComponent( ) {
            Initialize( );
            ConnectEvents( );
        }

        public void FocusTextInputArea( ) {
            InputBar.Focus( );
        }

        // Event handlers

        public void RoutedEvent_FocusInputBar( object sender, RoutedEventArgs ev ) {
            InputBar.Focus( );
        }

        public void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs ev ) {
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

        public void InputBar_LineInput( object sender, LineInputEventArgs ev ) {
            Debug.Print( "PanelImpl.InputBar_LineInput: input line is '{0}'", ev.Line );
            InputProcessor.Process( ev.Line );
        }

        public void InputBar_RelayedScroll( object sender, RelayedScrollEventArgs ev ) {
            ScrollActions[ ev.ScrollAction ]( );
        }

        //
        // Implementation
        //

        private Dictionary<ScrollAction, Action> ScrollActions;

        private void Initialize( ) {
            ScrollActions = new Dictionary<ScrollAction, Action> {
                { ScrollAction.PageUp,   ScrollViewer.PageUp         },
                { ScrollAction.PageDown, ScrollViewer.PageDown       },
                { ScrollAction.LineUp,   ScrollViewer.LineUp         },
                { ScrollAction.LineDown, ScrollViewer.LineDown       },
                { ScrollAction.Top,      ScrollViewer.ScrollToTop    },
                { ScrollAction.Bottom,   ScrollViewer.ScrollToBottom },
            };

            ScrollbackManager = new ScrollbackManager {
                FlowDocument = ScrollbackViewer.Document,
            };
        }

        private void ConnectEvents( ) {
            IrcPanel.GotFocus += RoutedEvent_FocusInputBar;
            ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            ScrollViewer.GotFocus += RoutedEvent_FocusInputBar;
            ScrollbackViewer.GotFocus += RoutedEvent_FocusInputBar;
            ScrollbackViewer.Document.GotFocus += RoutedEvent_FocusInputBar;
            InputBar.LineInput += InputBar_LineInput;
            InputBar.RelayedScroll += InputBar_RelayedScroll;
        }

    }

}
