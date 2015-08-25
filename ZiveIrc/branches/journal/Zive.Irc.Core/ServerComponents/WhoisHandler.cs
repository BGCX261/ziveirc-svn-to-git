using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace Zive.Irc.Core {

    public class WhoisHandler: ServerComponentBase {

        //
        // Interface
        //

        public event EventHandler<WhoisCompleteEventArgs> Complete;
        public event EventHandler<MessageEventArgs> Response;

        public WhoisHandler( Server server, EventHandler<WhoisCompleteEventArgs> completionHandler, MessageEventArgs ev ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "311" ), HandleRplWhoisUser  ),
                new FilterAndHandler( ( m ) => ( m.Verb == "318" ), HandleRplEndOfWhois ),
                new FilterAndHandler( ( m ) => ( true            ), HandleDefault       ),
            };
            Complete += completionHandler;
            Server = server;
            HandleRplWhoisUser( null, ev );
        }

        //
        // Implementation
        //

        private readonly Collection<Message> _responses = new Collection<Message>( );

        // Numeric 311
        public void HandleRplWhoisUser( object sender, MessageEventArgs ev ) {
            _responses.Clear( );
            _responses.Add( ev.Message );

            OnResponse( ev.Message );
        }

        // Numeric 318
        private void HandleRplEndOfWhois( object sender, MessageEventArgs ev ) {
            _responses.Add( ev.Message );

            OnResponse( ev.Message );
            OnComplete( new NickUserHost( ev.Message.Target ) );
        }

        private void HandleDefault( object sender, MessageEventArgs ev ) {
            int numeric = -1;
            if ( int.TryParse( ev.Message.Verb, NumberStyles.None, CultureInfo.InvariantCulture, out numeric ) ) {
                if ( 3 == ev.Message.Verb.Length && numeric >= 400 && numeric < 600 ) {
                    Debug.Print( "WhoisHandler.HandleDefault: Received numeric '{0}', aborting", ev.Message.Verb );
                    OnComplete( null );
                }
            }
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
