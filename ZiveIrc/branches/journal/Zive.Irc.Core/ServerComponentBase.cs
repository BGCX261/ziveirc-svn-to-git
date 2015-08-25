using System;
using System.Collections.Generic;

namespace Zive.Irc.Core {

    public abstract class ServerComponentBase: IServerComponent {

        //
        // Properties
        //

        public Server Server {
            get { return _server; }
            set {
                if ( null != _protocolHandler ) {
                    _protocolHandler.Unsubscribe( _messageFilters );
                }
                _server = value;
                if ( null != _server ) {
                    _protocolHandler = _server.ProtocolHandler;
                    _protocolHandler.Subscribe( _messageFilters );
                } else {
                    _protocolHandler = null;
                }
            }
        }

        //
        // Fields
        //

        // Property backing fields

        protected Server _server;
        protected ProtocolHandler _protocolHandler;

        // Other protected fields

        protected List<FilterAndHandler> _messageFilters;

    }

}
