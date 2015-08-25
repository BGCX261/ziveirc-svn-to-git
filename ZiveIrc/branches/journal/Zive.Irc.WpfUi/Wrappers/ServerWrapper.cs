using System;
using System.Diagnostics;
using Zive.Irc.Configuration;
using Zive.Irc.Core;

namespace Zive.Irc.WpfUi {

    public class ServerWrapper {

        public ServerWrapper( ) {

        }

        public ServerWrapper( ServerConfiguration serverConfiguration ) {
            ServerConfiguration = serverConfiguration;
        }

        public event EventHandler ConnectFailed;
        public event EventHandler ConnectStarted;
        public event EventHandler ConnectSucceeded;

        public ServerConfiguration ServerConfiguration { get; private set; }
        public string Name { get { return ServerConfiguration.Name; } }
        public int RefId { get { return ServerConfiguration.RefId; } }
        public NetworkWrapper NetworkWrapper { get; set; }
        public Server Server { get; set; }
        public bool IsConnected { get; set; }

        public void Connect( ) {
            if ( IsConnected ) {
                throw new InvalidOperationException( "Server is already connected" );
            }

            Debug.Print( "ServerWrapper.Connect: constructing _serverConnector" );
            _serverConnector = new ServerConnector( new ConnectionConfiguration {
                NickName = _configuration.UserConfiguration.NickName,
                UserName = _configuration.UserConfiguration.UserName,
                RealName = _configuration.UserConfiguration.RealName,
                NickServPassword = "timnpfns",
                ServerHostName = ServerConfiguration.HostName,
                Ports = ServerConfiguration.Ports,
            } );
            _serverConnector.ConnectionEstablished += HandleConnectionEstablished;
            _serverConnector.ConnectionAttemptStarted += HandleConnectionAttemptStarted;
            _serverConnector.ConnectionAttemptFailed += HandleConnectionAttemptFailed;
            _serverConnector.ConnectionFailed += HandleConnectionFailed;

            Debug.Print( "ServerWrapper.Connect: Calling _serverConnector.BeginConnect()" );
            try {
                _serverConnector.BeginConnect( );
            }
            catch ( Exception e ) {
                Debug.Print( "ServerWrapper.Connect: _serverConnector.BeginConnect threw an exception:\n{0}", e );
            }
        }

        public void Disconnect( ) {
            if ( !IsConnected ) {
                throw new InvalidOperationException( "Server is not connected" );
            }

            IsConnected = false;
        }

        private readonly ConfigurationRoot _configuration = App.Configuration;
        private ServerConnector _serverConnector;
        private bool _haveRaisedStartedEvent;

        private void CleanUp( ) {
            if ( null != _serverConnector ) {
                _serverConnector.Dispose( );
                _serverConnector = null;
            }
        }

        private void HandleConnectionAttemptStarted( object sender, ConnectionAttemptStartedEventArgs ev ) {
            Debug.Print( "ServerWrapper.HandleConnectionAttemptStarted" );
            Debug.Print( "+ remote end point: {0}", ev.SslEndPoint );
            if ( !_haveRaisedStartedEvent ) {
                _haveRaisedStartedEvent = true;
                OnConnectStarted( );
            }
        }

        private void HandleConnectionAttemptFailed( object sender, ConnectionAttemptFailedEventArgs ev ) {
            Debug.Print( "ServerWrapper.HandleConnectionAttemptFailed" );
            Debug.Print( "+ remote end point: {0}", ev.SslEndPoint );
            Debug.Print( "+ exception:\n{0}", ev.Exception );
        }

        private void HandleConnectionFailed( object sender, EventArgs ev ) {
            Debug.Print( "ServerWrapper.HandleConnectionFailed" );

            CleanUp( );

            OnConnectFailed( );
        }

        private void HandleConnectionEstablished( object sender, ConnectionEstablishedEventArgs ev ) {
            Debug.Print( "ServerWrapper.HandleConnectionEstablished" );
            Debug.Print( "+ remote end point: {0}", ev.Server.ServerEndPoint );

            Server = ev.Server;
            IsConnected = true;

            OnConnectSucceeded( );
        }

        private void OnConnectFailed( ) {
            var handler = ConnectFailed;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnConnectStarted( ) {
            var handler = ConnectStarted;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

        private void OnConnectSucceeded( ) {
            var handler = ConnectSucceeded;
            if ( null != handler ) {
                handler( this, EventArgs.Empty );
            }
        }

    }

}
