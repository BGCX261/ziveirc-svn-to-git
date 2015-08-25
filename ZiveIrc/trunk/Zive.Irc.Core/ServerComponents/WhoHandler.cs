using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Zive.Irc.Core {

    public class WhoHandler: ServerComponentBase {

        //
        // Interface
        //

        public Collection<Message> Responses { get { return _responses; } }
        public object Target { get; private set; }

        public event EventHandler Complete;
        public event EventHandler<MessageEventArgs> Response;

        public WhoHandler( ) {
            VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "352", HandleRplWhoReply },
                { "315", HandleRplEndOfWho },
            };
        }

        // Numeric 352
        public void HandleRplWhoReply( object sender, MessageEventArgs ev ) {
            if ( null == Target ) {
                var targetName = ev.Message.Args[ 0 ];
                Debug.Print( "WhoHandler.HandleRplWhoReply: Setting target to {0}", targetName );
                if ( Server.Channels.ContainsKey( targetName ) ) {
                    Target = Server.Channels[ targetName ];
                } else if ( Server.Users.ContainsKey( targetName ) ) {
                    Target = Server.Users[ targetName ];
                } else {
                    Target = null;
                }
            }

            _responses.Add( ev.Message );
            OnResponse( ev.Message );
        }

        //
        // Implementation
        //

        // Fields

        private readonly Collection<Message> _responses = new Collection<Message>( );

        // Methods

        // Numeric 315
        private void HandleRplEndOfWho( object sender, MessageEventArgs ev ) {
            OnResponse( ev.Message );
            OnComplete( );
        }

        // Event dispatchers

        private void OnComplete( ) {
            var handler = Complete;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnResponse( Message message ) {
            var handler = Response;
            if ( null != handler ) {
                handler( this, new MessageEventArgs( message ) );
            }
        }

    }

}
