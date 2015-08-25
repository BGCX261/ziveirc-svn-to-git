using System;
using System.Diagnostics;

namespace Zive.Irc.Core {

    public class SelfUser: User {

        //
        // Interface
        //

        // Properties

        public string NickServUserName { get; set; }
        public string NickServPassword { get; set; }

        public override Server Server {
            get { return _server; }
            set {
                if ( null != _server ) {
                    _server.Registered -= Server_Registered;
                }
                base.Server = value;
                if ( null != _server ) {
                    _server.Registered += Server_Registered;
                }
            }
        }

        public virtual bool IsRegistered {
            get { return _isRegistered; }
        }

        // Events

        public virtual event EventHandler<NoticeReceivedEventArgs> NoticeReceived;
        public virtual event EventHandler<PrivMsgReceivedEventArgs> PrivMsgReceived;
        public virtual event EventHandler<RegisteredEventArgs> Registered;

        // Constructors

        public SelfUser( ) {
            _messageFilters.AddRange( new FilterAndHandler[] {
                new FilterAndHandler( ( m ) => ( m.Verb == "MODE"                             && !m.Server.IsChannelName( m.Target ) ), HandleUserMode ),
                new FilterAndHandler( ( m ) => ( m.Verb == "NOTICE"  && !m.Origin.IsNickEmpty && !m.Server.IsChannelName( m.Target ) ), HandleNotice   ),
                new FilterAndHandler( ( m ) => ( m.Verb == "PRIVMSG"                          && !m.Server.IsChannelName( m.Target ) ), HandlePrivMsg  ),
            } );
        }

        // Methods

        //
        // Implementation
        //

        // Fields

        private bool _isRegistered;

        // Methods

        // Event handlers

        protected void Server_Registered( object sender, RegisteredEventArgs ev ) {
            _isRegistered = true;
            OnRegistered( sender, ev );
        }

        protected virtual void HandleNotice( object sender, MessageEventArgs ev ) {
            Debug.Print( "SelfUser.HandleNotice: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            if ( !_nickUserHost.NickName.Equals( ev.Message.Target, StringComparison.OrdinalIgnoreCase ) ) {
                Debug.Print( "+ Not for us: '{0}' vs '{1}'", ev.Message.Target, _nickUserHost.NickName );
                return;
            }
            if ( ev.Message.Origin.NickName.Equals( "NickServ", StringComparison.OrdinalIgnoreCase) ) {
                HandleNickServNotice( sender, ev );
                return;
            }
            OnNoticeReceived( sender, new NoticeReceivedEventArgs( _server.LookUpUser( ev.Message.Origin.NickName ), _server.LookUpTarget( ev.Message.Target ), ev.Message.Args[0] ) );
        }

        private void HandleNickServNotice( object sender, MessageEventArgs ev ) {
            if ( ev.Message.Args[0].StartsWith( "This nickname is registered.", StringComparison.OrdinalIgnoreCase ) ) {
                if ( string.IsNullOrWhiteSpace( NickServUserName ) && string.IsNullOrWhiteSpace( NickServPassword ) ) {
                    Debug.Print( "SelfUser.HandleNotice: NickServ has requested authentication, but we don't have any to give it. skipping." );
                    return;
                }
                Debug.Print( "SelfUser.HandleNotice: authenticating to NickServ. username='{0}' password='{1}'", NickServUserName ?? "", NickServPassword ?? "" );
                if ( string.IsNullOrWhiteSpace( NickServUserName ) ) {
                    _protocolHandler.SendToServer( "PRIVMSG NickServ :identify {0}", NickServPassword );
                } else {
                    _protocolHandler.SendToServer( "PRIVMSG NickServ :identify {0} {1}", NickServUserName, NickServPassword );
                }
            }
        }

        protected virtual void HandlePrivMsg( object sender, MessageEventArgs ev ) {
            Debug.Print( "SelfUser.HandlePrivMsg: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            if ( !_nickUserHost.NickName.Equals( ev.Message.Target, StringComparison.OrdinalIgnoreCase ) ) {
                Debug.Print( "+ Not for us: '{0}' vs '{1}'", ev.Message.Target, _nickUserHost );
                return;
            }
            OnPrivMsgReceived( sender, new PrivMsgReceivedEventArgs( _server.LookUpUser( ev.Message.Origin.NickName ), _server.LookUpTarget( ev.Message.Target ), ev.Message.Args[0] ) );
        }

        protected virtual void HandleUserMode( object sender, MessageEventArgs ev ) {
            Debug.Print( "SelfUser.HandleUserMode: target='{0}' mode change='{1}' mode args={2}", ev.Message.Target, ev.Message.Args[0], ev.Message.Args.Count > 1 ? "'" + ev.Message.Args[1] + "'" : "(none)" );

            var index = _userMode.IndexOf( ' ' );
            var currentMode = ( index > -1 ) ? _userMode.Substring( 0, index ) : _userMode;
            var positive = true;
            foreach ( var ch in ev.Message.Args[0] ) {
                switch ( ch ) {
                    case '+':
                        positive = true;
                        break;

                    case '-':
                        positive = false;
                        break;

                    default:
                        index = currentMode.IndexOf( ch );
                        if ( index > -1 ) {
                            // already in currentModes
                            if ( !positive ) {
                                currentMode = currentMode.Remove( index, 1 );
                            }
                        } else {
                            // not in currentModes
                            if ( positive ) {
                                currentMode += ch;
                            }
                        }
                        break;
                }
            }
            _userMode = currentMode;

            Debug.Print( "SelfUser.HandleUserMode: new user mode: {0}", _userMode );
            // TODO
        }

        // Event dispatchers

        private void OnNoticeReceived( object sender, NoticeReceivedEventArgs ev ) {
            var handler = NoticeReceived;
            if ( null != handler ) {
                handler( sender, ev );
            }
        }

        private void OnPrivMsgReceived( object sender, PrivMsgReceivedEventArgs ev ) {
            var handler = PrivMsgReceived;
            if ( null != handler ) {
                handler( sender, ev );
            }
        }

        private void OnRegistered( object sender, RegisteredEventArgs ev ) {
            var handler = Registered;
            if ( null != handler ) {
                handler( sender, ev );
            }
        }

    }

}
