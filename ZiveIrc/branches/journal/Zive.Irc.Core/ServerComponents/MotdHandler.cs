using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Zive.Irc.Core {

    public class MotdHandler: ServerComponentBase {

        //
        // Interface
        //

        public event EventHandler<MotdCompleteEventArgs> Complete;

        public MotdHandler( Server server, MessageEventArgs ev, EventHandler<MotdCompleteEventArgs> completionHandler ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "372" ), HandleRplMotd      ),
                new FilterAndHandler( ( m ) => ( m.Verb == "375" ), HandleRplMotdStart ),
                new FilterAndHandler( ( m ) => ( m.Verb == "376" ), HandleRplEndOfMotd ),
                new FilterAndHandler( ( m ) => ( m.Verb == "422" ), HandleErrNoMotd    ),
            };
            Complete += completionHandler;
            Server = server;

            if ( ev.Message.Verb == "375" ) {
                HandleRplMotdStart( null, ev );
            } else if ( ev.Message.Verb == "422" ) {
                HandleErrNoMotd( null, ev );
            } else {
                Debug.Print( "MotdHandler.`ctor: don't know what to do with verb '{0}'!", ev.Message.Verb );
                OnComplete( null );
            }
        }

        //
        // Implementation
        //

        private Collection<string> _lines;

        // Numeric 372
        private void HandleRplMotd( object sender, MessageEventArgs ev ) {
            _lines.Add( ev.Message.Args[ 0 ] );
        }

        // Numeric 375
        private void HandleRplMotdStart( object sender, MessageEventArgs ev ) {
            _lines = new Collection<string>( );
        }

        // Numeric 376
        private void HandleRplEndOfMotd( object sender, MessageEventArgs ev ) {
            OnComplete( _lines );
        }

        // Numeric 422
        private void HandleErrNoMotd( object sender, MessageEventArgs ev ) {
            OnComplete( null );
        }

        private void OnComplete( Collection<string> lines ) {
            var handler = Complete;
            if ( null != handler ) {
                handler( this, new MotdCompleteEventArgs( lines ) );
            }
        }

    }

}
