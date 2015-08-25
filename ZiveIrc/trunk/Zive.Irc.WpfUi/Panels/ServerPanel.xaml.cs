using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Zive.Irc.Core;

namespace Zive.Irc.WpfUi {

    public partial class ServerPanel: UserControl, IIrcPanel {

        //
        // Interface
        //

        // Fields

        // Properties

        public InputProcessor InputProcessor {
            get { return _inputProcessor; }
            set { _inputProcessor = value; }
        }

        public ScrollbackManager ScrollbackManager {
            get { return _scrollbackManager; }
        }

        public NetworkWrapper NetworkWrapper {
            get { return _networkWrapper; }
            set { _networkWrapper = value; }
        }

        public ServerWrapper ServerWrapper {
            get { return _serverWrapper; }
            set {
                if ( null != _serverWrapper ) {
                    _UnhookServerEvents( );
                }
                _serverWrapper = value;
                if ( null != _serverWrapper ) {
                    _HookUpServerEvents( );
                }
            }
        }

        public Server Server {
            get { return _serverWrapper.Server; }
        }

        // Events

        // Constructors

        public ServerPanel( ) {
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
        }

        // Methods

        public void FocusInputBar( ) {
            InputBar.Focus( );
        }

        //
        // Implementation
        //

        // Fields

        private InputProcessor _inputProcessor;
        private ScrollbackManager _scrollbackManager;
        private NetworkWrapper _networkWrapper;
        private ServerWrapper _serverWrapper;

        private readonly Dictionary<ScrollAction, Action> _scrollActions;

        // Properties

        // Events

        // Constructors

        // Methods

        private void _HookUpServerEvents( ) {
            Server.MotdComplete  += Server_MotdComplete;
            Server.NamesComplete += Server_NamesComplete;
            Server.Registered    += Server_Registered;
            Server.WhoComplete   += Server_WhoComplete;
            Server.WhoisComplete += Server_WhoisComplete;
            Server.Unknown       += Server_Unknown;
        }

        private void _UnhookServerEvents( ) {
            Server.MotdComplete  -= Server_MotdComplete;
            Server.NamesComplete -= Server_NamesComplete;
            Server.Registered    -= Server_Registered;
            Server.WhoComplete   -= Server_WhoComplete;
            Server.WhoisComplete -= Server_WhoisComplete;
            Server.Unknown       -= Server_Unknown;
        }

        // IRC event handlers

        private void Server_MotdComplete( object sender, MotdCompleteEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_MotdComplete" );

            Dispatcher.BeginInvoke( (Action) ( ( ) => {
                _scrollbackManager.AddLine( ScrollbackParagraphMaker.Make(
                    "Server's ",
                    FontStyles.Italic,
                    "Message of the Day",
                    ":"
                ) );

                var style = FindResource( "MessageOfTheDayStyle" ) as Style;
                var para = ScrollbackParagraphMaker.Make( string.Join( "\r\n", ev.Lines.Select( _ => _.StartsWith( "- " ) ? _.Substring( 2 ) : _ ) ) );
                para.Style = style;
                _scrollbackManager.AddLine( para );
            } ) );
        }

        private void Server_NamesComplete( object sender, NamesCompleteEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_NamesComplete" );
        }

        private void Server_Registered( object sender, RegisteredEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_Registered" );
            Dispatcher.BeginInvoke( (Action) ( ( ) => _scrollbackManager.AddLine( ScrollbackParagraphMaker.Make( "Registered." ) ) ) );
        }

        private void Server_WhoComplete(object sender, WhoCompleteEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_WhoComplete" );
        }

        private void Server_WhoisComplete(object sender, WhoisCompleteEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_WhoisComplete" );
        }

        private void Server_Unknown(object sender, MessageEventArgs ev ) {
            Debug.Print( "ServerPanel.Server_Unknown" );

            Message m = ev.Message;
            Dispatcher.BeginInvoke( (Action) ( ( ) => {
                var style = FindResource( "RawMessageStyle" ) as Style;
                var para = ScrollbackParagraphMaker.Make( m.ToString( ) );
                para.Style = style;
                _scrollbackManager.AddLine( para );
            } ) );
        }

        // Polyvalent UI event handlers

        private void RoutedEvent_GotFocus( object sender, RoutedEventArgs ev ) {
            InputBar.Focus( );
        }

        // Specific UI event handlers

        private void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs ev ) {
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

        private void InputBar_LineInput( object sender, LineInputEventArgs ev ) {
            Debug.Print( "ConsolePanel.InputBar_LineInput: input line is '{0}'", ev.Line );
            _inputProcessor.Process( ev.Line );
        }

        private void InputBar_RelayedScroll( object sender, RelayedScrollEventArgs ev ) {
            _scrollActions[ ev.ScrollAction ]( );
        }

    }

}
