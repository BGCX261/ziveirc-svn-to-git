using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class TargetedEvent {

        public string EventKey;
        public EventHandler<MessageEventArgs> Handler;

    }

    public class TargetedEventManager {

        protected Dictionary<string, Dictionary<object, EventHandler<MessageEventArgs>>> Delegates = new Dictionary<string, Dictionary<object, EventHandler<MessageEventArgs>>>( );

        public virtual void Subscribe( TargetedEvent targetedEvent ) {
            Subscribe( targetedEvent.EventKey, targetedEvent.Handler );
        }

        public virtual void Subscribe( string eventKey, EventHandler<MessageEventArgs> handler ) {
            Dictionary<object, EventHandler<MessageEventArgs>> dict;
            if ( Delegates.ContainsKey( eventKey ) ) {
                dict = Delegates[ eventKey ];
            } else {
                dict = Delegates[ eventKey ] = new Dictionary<object, EventHandler<MessageEventArgs>>( );
            }

            dict.ReplaceOrAdd( handler.Target, handler );
        }

        public virtual void Unsubscribe( TargetedEvent targetedEvent ) {
            Unsubscribe( targetedEvent.EventKey, targetedEvent.Handler );
        }

        public virtual void Unsubscribe( string eventKey, EventHandler<MessageEventArgs> handler ) {
            if ( Delegates.ContainsKey( eventKey ) ) {
                Delegates[ eventKey ].RemoveIfContains( handler.Target );
            } else {
                Delegates[ eventKey ] = new Dictionary<object, EventHandler<MessageEventArgs>>( );
            }
        }

        public virtual void BulkSubscribe( IList<TargetedEvent> events ) {
            foreach ( var ev in events ) {
                Subscribe( ev );
            }
        }

        public virtual void BulkUnsubscribe( IList<TargetedEvent> events ) {
            foreach ( var ev in events ) {
                Unsubscribe( ev );
            }
        }

        public virtual Dictionary<object, EventHandler<MessageEventArgs>> this[ string eventKey ] {
            get {
                Dictionary<object, EventHandler<MessageEventArgs>> dict;
                if ( Delegates.ContainsKey( eventKey ) ) {
                    dict = Delegates[ eventKey ];
                } else {
                    dict = Delegates[ eventKey ] = new Dictionary<object, EventHandler<MessageEventArgs>>( );
                }
                return dict;
            }
            set {
                Delegates[ eventKey ] = value;
            }
        }

    }

}
