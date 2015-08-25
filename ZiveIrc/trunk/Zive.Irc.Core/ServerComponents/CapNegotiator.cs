using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public CapNegotiator( ) {
            VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "CAP", HandleCap },
            };
            _capSubverbs = new Dictionary<string, Action<object, MessageEventArgs>> {
                { "LS",    HandleCapLs    },
                { "LIST",  HandleCapList  },
                { "REQ",   HandleCapReq   },
                { "ACK",   HandleCapAck   },
                { "NAK",   HandleCapNak   },
                { "CLEAR", HandleCapClear },
                { "END",   HandleCapEnd   },
            };
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

        private readonly Dictionary<string,Action<object,MessageEventArgs>> _capSubverbs;
        private readonly List<string> _capSendList = new List<string>( );
        private readonly List<string> _capWaitList = new List<string>( );

        private bool _haveSentLs;
        private bool _haveSentList;
        private bool _haveSentEnd;

        // Methods

        private void HandleCap( object sender, MessageEventArgs ev ) {
            if ( _capSubverbs.ContainsKey( ev.Message.Args[ 0 ] ) ) {
                _capSubverbs[ ev.Message.Args[ 0 ] ]( sender, ev );
            }
        }

        private void HandleCapLs( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapLs: arg='{0}'", ev.Message.Args[ 1 ] );
            if ( !string.IsNullOrWhiteSpace( ev.Message.Args[ 1 ] ) ) {
                string[ ] avail = ev.Message.Args[ 1 ].Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries );
                _availableCapabilities.AddRange( avail );
                _capSendList.AddRange( avail.Where( cap => KnownFeatures.Contains( cap ) ) );
            }
            if ( !_haveSentList ) {
                Server.ProtocolHandler.SendToServer( "CAP LIST" );
                _haveSentList = true;
            }
        }

        private void HandleCapList( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", ev.Message.Args[ 1 ] );
            if ( !string.IsNullOrWhiteSpace( ev.Message.Args[ 1 ] ) ) {
                string[ ] enabled = ev.Message.Args[ 1 ].Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries );
                _enabledCapabilities.AddRange( enabled );
                foreach ( var cap in enabled ) {
                    _capSendList.Remove( cap );
                }
            }
            TrySendingRequest( );
        }

        private void HandleCapReq( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapReq: huh? '{0}'", ev.Message );
        }

        private void HandleCapAck( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[ 1 ].Trim( );
            Debug.Print( "CapNegotiator.HandleCapAck: arg='{0}'", arg );
            _enabledCapabilities.Add( arg );
            _capWaitList.Remove( arg );
            if ( _haveSentEnd && _capWaitList.Count < 1 ) {
                OnComplete( );
                return;
            }
            TrySendingRequest( );
        }

        private void HandleCapNak( object sender, MessageEventArgs ev ) {
            var arg = ev.Message.Args[ 1 ].Trim( );
            Debug.Print( "CapNegotiator.HandleCapNak: arg='{0}'", arg );
            _capWaitList.Remove( arg );
            if ( _haveSentEnd && _capWaitList.Count < 1 ) {
                OnComplete( );
                return;
            }
            TrySendingRequest( );
        }

        private void HandleCapClear( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", ev.Message.Args[ 1 ] );
            _enabledCapabilities.Clear( );
        }

        private void HandleCapEnd( object sender, MessageEventArgs ev ) {
            Debug.Print( "CapNegotiator.HandleCapList: arg='{0}'", ev.Message.Args[ 1 ] );
            OnComplete( );
        }

        private void TrySendingRequest( ) {
            if ( !_haveSentEnd && _capSendList.Count < 1 ) {
                Server.ProtocolHandler.SendToServer( "CAP END" );
                _haveSentEnd = true;
                if ( _capWaitList.Count < 1 ) {
                    OnComplete( );
                }
                return;
            }
            var cap = _capSendList[ 0 ];
            _capSendList.RemoveAt( 0 );
            Server.ProtocolHandler.SendToServer( "CAP REQ :{0}", cap );
            _capWaitList.Add( cap );
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
