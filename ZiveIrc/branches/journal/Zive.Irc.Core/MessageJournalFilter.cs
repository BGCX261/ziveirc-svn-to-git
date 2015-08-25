using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Zive.Irc.Core {

    using FilterFuncType  = Func<Message, bool>;
    using HandlerFuncType = EventHandler<MessageEventArgs>;

    public class MessageJournalFilter: IDisposable {

        //
        // Interface
        //

        // Constructors

        public MessageJournalFilter( MessageJournal journal ) {
            if ( null == journal ) {
                throw new ArgumentNullException( "journal" );
            }
            _journal = journal;
            _SubscribeToCollectionChangedEvent( );
        }

        public MessageJournalFilter( MessageJournal journal, FilterFuncType filter, HandlerFuncType handler ) : this( journal ) {
            Subscribe( filter, handler );
        }

        public MessageJournalFilter( MessageJournal journal, IList<FilterAndHandler> list ) : this( journal ) {
            Subscribe( list );
        }

        // Properties

        public MessageJournal Journal { get { return _journal; } }
        public List<FilterAndHandler> Filters { get { return _filters; } }

        // Events

        // Methods

        public void Subscribe( FilterFuncType filter, HandlerFuncType handler ) {
            _filters.Add( new FilterAndHandler( filter, handler ) );
        }

        public void Subscribe( IList<FilterAndHandler> list ) {
            foreach ( var item in list ) {
                Subscribe( item.Filter, item.Handler );
            }
        }

        public void Unsubscribe( FilterFuncType filter, HandlerFuncType handler ) {
            _filters.Remove( new FilterAndHandler( filter, handler ) );
        }

        public void Unsubscribe( IList<FilterAndHandler> list ) {
            foreach ( var item in list ) {
                Unsubscribe( item.Filter, item.Handler );
            }
        }

        public void Suspend( ) {
            _UnsubscribeFromCollectionChangedEvent( );
        }

        public void Resume( ) {
            _SubscribeToCollectionChangedEvent( );
        }

        //
        // Implementation
        //

        private MessageJournal _journal;
        private List<FilterAndHandler> _filters = new List<FilterAndHandler>( );

        private void _SubscribeToCollectionChangedEvent( ) {
            _journal.CollectionChanged += _HandleCollectionChanged;
        }

        private void _UnsubscribeFromCollectionChangedEvent( ) {
            _journal.CollectionChanged -= _HandleCollectionChanged;
        }

        private void _HandleCollectionChanged( object sender, NotifyCollectionChangedEventArgs ev ) {
            if ( ev.Action != NotifyCollectionChangedAction.Add ) {
                Debug.Print( "MessageJournalFilter._HandleCollectionChanged: Ignoring action '{0}'", ev.Action );
                return;
            }

            foreach ( var m in ev.NewItems.Cast<Message>( ) ) {
                foreach ( var kv in _filters ) {
                    var filter = kv.Filter;
                    var handler = kv.Handler;
                    if ( null != filter && null != handler ) {
                        if ( filter( m ) ) {
                            handler( this, new MessageEventArgs( m ) );
                        }
                    }
                }
            }
        }

        //
        // IDisposable
        //

        private bool disposedValue = false;

        protected virtual void Dispose( bool disposing ) {
            if ( !disposedValue ) {
                if ( disposing ) {
                    _UnsubscribeFromCollectionChangedEvent( );
                    _journal = null;
                    _filters = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose( ) {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
        }

    }

}
