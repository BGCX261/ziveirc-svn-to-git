using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zive.Irc.Core {

    public class WhoisHandler: ServerComponentBase {

        //
        // Interface
        //

        public event EventHandler<WhoisCompleteEventArgs> Complete;
        public event EventHandler<MessageEventArgs> Response;

        public WhoisHandler( ) {
            VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "311", HandleRplWhoisUser },
                { "318", HandleRplEndOfWhois },
                { string.Empty, HandleDefault },
            };
        }

        // Numeric 311
        public void HandleRplWhoisUser( object sender, MessageEventArgs ev ) {
            _responses.Clear( );
            _responses.Add( ev.Message );

            OnResponse( ev.Message );
        }

        //
        // Implementation
        //

        private readonly Collection<Message> _responses = new Collection<Message>( );

        // Numeric 318
        private void HandleRplEndOfWhois( object sender, MessageEventArgs ev ) {
            _responses.Add( ev.Message );

            OnResponse( ev.Message );
            OnComplete( new NickUserHost( ev.Message.Target ) );
        }

        private void HandleDefault( object sender, MessageEventArgs ev ) {
            _responses.Add( ev.Message );

            OnResponse( ev.Message );
        }

        private void OnComplete( NickUserHost target ) {
            var handler = Complete;
            if ( null != handler ) {
                handler( this, new WhoisCompleteEventArgs( target, _responses ) );
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
