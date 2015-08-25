using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Zive.Irc.Configuration;
using Zive.Irc.Core;

namespace Zive.Irc.WpfUi {

    public class NetworkWrapper {

        public NetworkWrapper( ) {

        }

        public NetworkWrapper( NetworkConfiguration networkConfig ) {
            NetworkConfiguration = networkConfig;
            try {
                Servers = new Collection<ServerWrapper>( networkConfig.Servers.Select( _ => new ServerWrapper( _ ) ).ToList( ) );
            }
            catch ( Exception e ) {
                Debug.Print( "NetworkWrapper.`ctor: caught exception:\n{0}", e );
            }
        }

        public event EventHandler<EventArgs> ConnectFailed;
        public event EventHandler<EventArgs> ConnectStarted;
        public event EventHandler<EventArgs> ConnectSucceeded;
        public event EventHandler<ServerChangeEventArgs> ServerChange;

        public NetworkConfiguration NetworkConfiguration { get; private set; }
        public string Name { get { return NetworkConfiguration.Name; } }
        public int RefId { get { return NetworkConfiguration.RefId; } }
        public Collection<ServerWrapper> Servers { get; private set; }

        public bool IsConnected { get; set; }

        private bool _haveRaisedStartedEvent;
        private IEnumerator<ServerWrapper> _serverEnumerator;
        private ServerWrapper _currentServer;
        private ServerConnector _serverConnector;

        public void Connect( ) {
            Debug.Print( "NetworkWrapper.Connect: IsConnected={0}", IsConnected );
            if ( IsConnected ) {
                throw new InvalidOperationException( "Network is already connected" );
            }

            Debug.Print( "NetworkWrapper.Connect: _serverEnumerator={0}", _serverEnumerator );
            if ( null == _serverEnumerator ) {
                _serverEnumerator = Servers.GetEnumerator( );
            }

            Debug.Print( "NetworkWrapper.Connect: advancing enumerator" );
            if ( !_serverEnumerator.MoveNext( ) ) {
                Debug.Print( "NetworkWrapper.Connect: out of server:port pairs to try." );
                _haveRaisedStartedEvent = false;
                return;
            }
            _currentServer = _serverEnumerator.Current;
            OnServerChange( _currentServer );

            Debug.Print( "NetworkWrapper.Connect: constructing _serverConnector" );
            _serverConnector = new ServerConnector( new ConnectionConfiguration {
                NickName = "ZiveIrcTest",
                UserName = "icekarma",
                RealName = "Testing instance of ZiveIrc. Contact: IceKarma",
                NickServPassword = "timnpfns",
                ServerHostName = _currentServer.ServerConfiguration.HostName,
                Ports = _currentServer.ServerConfiguration.Ports,
            } );
            _serverConnector.ConnectionEstablished += ConnectionEstablished;
            _serverConnector.ConnectionAttemptStarted += ConnectionAttemptStarted;
            _serverConnector.ConnectionAttemptFailed += ConnectionAttemptFailed;
            _serverConnector.ConnectionFailed += ConnectionFailed;

            Debug.Print( "NetworkWrapper.Connect: Calling _serverConnector.BeginConnect()" );
            try {
                _serverConnector.BeginConnect( );
            }
            catch ( Exception e ) {
                Debug.Print( "NetworkWrapper.Connect: _serverConnector.BeginConnect threw an exception:\n{0}", e );
            }
        }

        public void Disconnect( ) {
            if ( !IsConnected ) {
                throw new InvalidOperationException( "Network is not connected" );
            }

            IsConnected = false;
        }

        private void ConnectionAttemptStarted( object sender, ConnectionAttemptStartedEventArgs ev ) {
            Debug.Print( "NetworkWrapper.ConnectionAttemptStarted" );
            Debug.Print( "+ remote end point: {0}", ev.SslEndPoint );
            if ( !_haveRaisedStartedEvent ) {
                _haveRaisedStartedEvent = true;
                OnConnectAttemptStarted( );
            }
        }

        private void ConnectionAttemptFailed( object sender, ConnectionAttemptFailedEventArgs ev ) {
            Debug.Print( "NetworkWrapper.ConnectionAttemptFailed" );
            Debug.Print( "+ remote end point: {0}", ev.SslEndPoint );
            Debug.Print( "+ exception:\n{0}", ev.Exception );
        }

        private void ConnectionFailed( object sender, EventArgs ev ) {
            Debug.Print( "NetworkWrapper.ConnectionFailed" );

            if ( null != _serverConnector ) {
                _serverConnector.Dispose( );
                _serverConnector = null;
            }

            OnConnectAttemptFailed( );
        }

        private void ConnectionEstablished( object sender, ConnectionEstablishedEventArgs ev ) {
            Debug.Print( "NetworkWrapper.ConnectionEstablished" );
            Debug.Print( "+ remote end point: {0}", ev.Server.ServerEndPoint );

            OnConnectAttemptSucceeded( );
        }

        private void OnConnectAttemptFailed( ) {
            var handler = ConnectFailed;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnConnectAttemptStarted( ) {
            var handler = ConnectStarted;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnConnectAttemptSucceeded( ) {
            var handler = ConnectSucceeded;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnServerChange( ServerWrapper newServerHostName ) {
            var handler = ServerChange;
            if ( null != handler ) {
                handler( this, new ServerChangeEventArgs( newServerHostName ) );
            }
        }

    }

}
