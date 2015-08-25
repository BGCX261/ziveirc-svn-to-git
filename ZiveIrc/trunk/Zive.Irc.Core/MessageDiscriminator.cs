using System;
using System.Collections.Generic;

namespace Zive.Irc.Core {

    public class MessageDiscriminator {

        private readonly Dictionary<string, EventHandler<MessageEventArgs>> _eventHandlers = new Dictionary<string, EventHandler<MessageEventArgs>>( );

        public EventHandler<MessageEventArgs> this[ string index ] {
            get {
                lock ( _eventHandlers ) {
                    return _eventHandlers.ContainsKey( index ) ? _eventHandlers[ index ] : null;
                }
            }
            set {
                lock ( _eventHandlers ) {
                    if ( _eventHandlers.ContainsKey( index ) ) {
                        _eventHandlers[ index ] += value;
                    } else {
                        _eventHandlers.Add( index, value );
                    }
                }
            }
        }

        public EventHandler<MessageEventArgs> this[ Message index ] {
            get {
                lock ( _eventHandlers ) {
                    return _eventHandlers.ContainsKey( index.Verb ) ? _eventHandlers[ index.Verb ] : null;
                }
            }
            set {
                lock ( _eventHandlers ) {
                    if ( _eventHandlers.ContainsKey( index.Verb ) ) {
                        _eventHandlers[ index.Verb ] += value;
                    } else {
                        _eventHandlers.Add( index.Verb, value );
                    }
                }
            }
        }

    }

}
