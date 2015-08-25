using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class CapNegotiator: ServerComponentBase {

        //
        // Interface
        //

        public virtual ICollection<string> AvailableCapabilities { get { return _availableCapabilities; } }
        public virtual ICollection<string> EnabledCapabilities { get { return _enabledCapabilities; } }

        // Events

        public event EventHandler Complete;

        // Constructor

        private CapNegotiator( ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "LS"    ), HandleCapLs           ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "LIST"  ), HandleCapList         ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "REQ"   ), HandleCapReq          ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "ACK"   ), HandleCapAck          ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "NAK"   ), HandleCapNak          ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "CLEAR" ), HandleCapClear        ),
                new FilterAndHandler( ( m ) => ( m.Verb == "CAP" && m.Args[0] == "END"   ), HandleCapEnd          ),
                new FilterAndHandler( ( m ) => ( m.Verb == "410"                         ), HandleErrInvalidCapCmd ),

                new FilterAndHandler( ( m ) => ( ( 3 == m.Verb.Length ) && ( '4' == m.Verb[0] || '5' == m.Verb[0] ) ), HandleError ),
            };
        }

        public CapNegotiator( Server server, EventHandler completionHandler ) : this( ) {
            Complete += completionHandler;
            Server = server;
        }

        public void Start( ) {
            if ( _haveSentLs ) {
                return;
            }

            Server.ProtocolHandler.SendToServer( "CAP LS" );
            _haveSentLs = true;
        }

        //
        // Implementation
        //

        private static readonly List<string> KnownFeatures = new List<string> {
            "account-notify",
            "away-notify",
            "multi-prefix",
            "tls",
            "userhost-in-names",
        };

        // Protected fields

        protected List<string> _availableCapabilities = new List<string>( );
        protected List<string> _enabledCapabilities = new List<string>( );

        // Private static readonly fields

        private readonly List<string> _capSendList = new List<string>( );
        private readonly List<string> _capWaitList = new List<string>( );

        private bool _haveSentLs;
        private bool _haveSentList;
        private bool _haveSentEnd;

        // Methods

        private void HandleCapLs( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[1];
            Debug.Print( "CapNegotiator.HandleCapLs: arg='{0}'", arg );
            if ( !string.IsNullOrWhiteSpace( arg ) ) {
                string[] avail = arg.Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries );
                _availableCapabilities.AddRange( avail );
                _capSendList.AddRange( avail.Where( cap => KnownFeatures.Contains( cap ) ) );
            }
            if ( !_haveSentList ) {
                Server.ProtocolHandler.SendToServer( "CAP LIST" );
                _haveSentList = true;
            }
        }

        private void HandleCapList( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[1];
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", arg );
            if ( !string.IsNullOrWhiteSpace( arg ) ) {
                string[] enabled = arg.Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries );
                _enabledCapabilities.AddRange( enabled );
                foreach ( var cap in enabled ) {
                    _capSendList.Remove( cap );
                }
            }
            TryRequestCapability( );
        }

        private void HandleCapReq( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapReq: huh? message: {0}", ev.Message );
        }

        private void HandleCapAck( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[1].Trim( );
            Debug.Print( "CapNegotiator.HandleCapAck: arg='{0}'", arg );
            _enabledCapabilities.Add( arg );
            _capWaitList.Remove( arg );
            if ( _haveSentEnd && _capWaitList.Count < 1 ) {
                OnComplete( );
                return;
            }
            TryRequestCapability( );
        }

        private void HandleCapNak( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[1].Trim( );
            Debug.Print( "CapNegotiator.HandleCapNak: arg='{0}'", arg );
            _capWaitList.Remove( arg );
            if ( _haveSentEnd && _capWaitList.Count < 1 ) {
                OnComplete( );
                return;
            }
            TryRequestCapability( );
        }

        private void HandleCapClear( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", ev.Message.Args[1] );
            _enabledCapabilities.Clear( );
        }

        private void HandleCapEnd( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", ev.Message.Args[1] );
            OnComplete( );
        }

        private void HandleErrInvalidCapCmd( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleErrInvalidCapCmd: message={0}", ev.Message );
            OnComplete( );
        }

        private void HandleError( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleError: message='{0}'", ev.Message );
            OnComplete( );
        }

        private void TryRequestCapability( ) {
            Debug.Print( "CapNegotiator.TryRequestCapability:" );
            if ( !_haveSentEnd && _capSendList.Count < 1 ) {
                Debug.Print( "+ Sending CAP END" );
                Server.ProtocolHandler.SendToServer( "CAP END" );
                _haveSentEnd = true;
                if ( _capWaitList.Count < 1 ) {
                    Debug.Print( "+ _capWaitList is empty -- firing Complete" );
                    OnComplete( );
                } else {
                    Debug.Print( "+ _capWaitList still contains '{0}' -- NOT firing Complete", string.Join( "','", _capWaitList ) );
                }
            } else {
                var cap = _capSendList[0];
                Debug.Print( "+ Sending request for capability '{0}'", cap );
                _capSendList.RemoveAt( 0 );
                Server.ProtocolHandler.SendToServer( "CAP REQ :{0}", cap );
                _capWaitList.Add( cap );
            }
        }

        // Event dispatchers

        private void OnComplete( ) {
            var handler = Complete;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

    }

}
