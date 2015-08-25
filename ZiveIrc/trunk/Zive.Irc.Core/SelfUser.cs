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

        // Events

        public event EventHandler<MessageEventArgs> Error;
        public event EventHandler<MessageEventArgs> Quit;

        // Constructors

        // Methods

        //
        // Implementation
        //

        // Methods

        protected override void RegisterServerHandlers( ) {
            base.RegisterServerHandlers( );
            _server.EventManager.Subscribe( "ERROR", HandleError );
        }

        protected override void UnregisterServerHandlers( ) {
            _server.EventManager.Unsubscribe( "ERROR", HandleError );
            base.UnregisterServerHandlers( );
        }

        // Event handlers

        protected virtual void HandleError( object sender, MessageEventArgs ev ) {
            OnError( ev.Message );
        }

        protected override void HandleNotice( object sender, MessageEventArgs ev ) {
            Debug.Print( "SelfUser.HandleNotice: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            if ( ev.Message.Origin.NickName.Equals( "NickServ", StringComparison.OrdinalIgnoreCase ) ) {
                if ( ev.Message.Args[ 0 ].StartsWith( "This nickname is registered.", StringComparison.OrdinalIgnoreCase ) ) {
                    if ( string.IsNullOrWhiteSpace( NickServUserName ) && string.IsNullOrWhiteSpace( NickServPassword ) ) {
                        Debug.Print( "SelfUser.HandleNotice: NickServ has requested authentication, but we don't have any to give it. skipping." );
                        return;
                    }
                    Debug.Print( "SelfUser.HandleNotice: authenticating to NickServ. username='{0}' password='{1}'", NickServUserName ?? "", NickServPassword ?? "" );
                    if ( string.IsNullOrWhiteSpace( NickServUserName ) ) {
                        ProtocolHandler.SendToServer( "PRIVMSG NickServ :identify {0}", NickServPassword );
                    } else {
                        ProtocolHandler.SendToServer( "PRIVMSG NickServ :identify {0} {1}", NickServUserName, NickServPassword );
                    }
                }
            }
        }

        protected override void HandlePrivMsg( object sender, MessageEventArgs ev ) {
            Debug.Print( "SelfUser.HandlePrivMsg: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
        }

        protected override void HandleQuit( object sender, MessageEventArgs ev ) {
            OnQuit( ev.Message );
        }

        // Event dispatchers

        protected virtual void OnError( Message message ) {
            var handler = Error;
            if ( null != handler ) {
                handler( this, new MessageEventArgs( message ) );
            }
        }

        protected virtual void OnQuit( Message message ) {
            var handler = Quit;
            if ( null != handler ) {
                handler( this, new MessageEventArgs( message ) );
            }
        }

    }

}
