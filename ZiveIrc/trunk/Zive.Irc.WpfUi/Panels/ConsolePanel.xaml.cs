using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Zive.Irc.WpfUi {

    public partial class ConsolePanel: UserControl, IIrcPanel {

        //
        // Interface
        //

        public ScrollbackManager ScrollbackManager { get { return _scrollbackManager; } }

        public InputProcessor InputProcessor { get; set; }

        public ConsolePanel( ) {
            InitializeComponent( );

            _scrollActions = new Dictionary<ScrollAction, Action> {
                { ScrollAction.PageUp,   ScrollViewer.PageUp         },
                { ScrollAction.PageDown, ScrollViewer.PageDown       },
                { ScrollAction.LineUp,   ScrollViewer.LineUp         },
                { ScrollAction.LineDown, ScrollViewer.LineDown       },
                { ScrollAction.Top,      ScrollViewer.ScrollToTop    },
                { ScrollAction.Bottom,   ScrollViewer.ScrollToBottom },
            };

            _scrollbackManager = new ScrollbackManager {
                FlowDocument = ScrollbackViewer.Document,
            };
            _scrollbackManager.AddLine( "Welcome to ZiveIRC." );
        }

        public void FocusInputBar( ) {
            InputBar.Focus( );
        }

        //
        // Implementation
        //

        protected ScrollbackManager _scrollbackManager;

        // Private methods

        // Command handlers

        protected readonly Dictionary<ScrollAction, Action> _scrollActions;

        // Polyvalent event handlers

        protected void RoutedEvent_GotFocus( object sender, RoutedEventArgs ev ) {
            InputBar.Focus( );
        }

        // Specific event handlers

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
            Debug.Print( "ConsolePanel.InputBar_LineInput: input line is '{0}'", ev.Line );
            InputProcessor.Process( ev.Line );
        }

        protected void InputBar_RelayedScroll( object sender, RelayedScrollEventArgs ev ) {
            _scrollActions[ ev.ScrollAction ]( );
        }

    }

}
