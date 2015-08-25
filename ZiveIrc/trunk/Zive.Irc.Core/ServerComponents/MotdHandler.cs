using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zive.Irc.Core {

    public class MotdHandler: ServerComponentBase {

        //
        // Interface
        //

        public event EventHandler<MotdCompleteEventArgs> Complete;

        public MotdHandler( ) {
            VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "372", HandleRplMotd },
                { "376", HandleRplEndOfMotd },
                { "422", HandleErrNoMotd },
            };
        }

        // Numeric 375
        public void HandleRplMotdStart( object sender, MessageEventArgs ev ) {
            _lines = new Collection<string>( );
        }

        //
        // Implementation
        //

        private Collection<string> _lines;

        // Numeric 372
        private void HandleRplMotd( object sender, MessageEventArgs ev ) {
            _lines.Add( ev.Message.Args[ 0 ] );
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
