using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Zive.Irc.Configuration;

namespace Zive.Irc.WpfUi {

    public class NetworkWrapper {

        public NetworkWrapper( ) {

        }

        public NetworkWrapper( NetworkConfiguration networkConfig ) {
            NetworkConfiguration = networkConfig;

            try {
                Servers = new Collection<ServerWrapper>( networkConfig.Servers.Select( _ => {
                    var wrapper = new ServerWrapper( _ ) {
                        NetworkWrapper = this,
                    };
                    wrapper.ConnectFailed += HandleConnectFailed;
                    wrapper.ConnectStarted += HandleConnectStarted;
                    wrapper.ConnectSucceeded += HandleConnectSucceeded;
                    return wrapper;
                } ).ToList( ) );
            }
            catch ( Exception e ) {
                Debug.Print( "NetworkWrapper.`ctor: caught exception:\n{0}", e );
            }
        }

        public event EventHandler ConnectFailed;
        public event EventHandler ConnectStarted;
        public event EventHandler ConnectSucceeded;
        public event EventHandler<ServerChangeEventArgs> ServerChange;

        public NetworkConfiguration NetworkConfiguration { get; private set; }
        public string Name { get { return NetworkConfiguration.Name; } }
        public int RefId { get { return NetworkConfiguration.RefId; } }
        public Collection<ServerWrapper> Servers { get; private set; }

        public bool IsConnected { get; set; }

        private IEnumerator<ServerWrapper> _serverEnumerator;
        private ServerWrapper _currentServer;
        private bool _haveRaisedStartedEvent;

        public void Connect( ) {
            Debug.Print( "NetworkWrapper.Connect: IsConnected={0}", IsConnected );
            if ( IsConnected ) {
                Debug.Print( "NetworkWrapper.Connect: already connected" );
                return;
            }

            Debug.Print( "NetworkWrapper.Connect: _serverEnumerator={0}", _serverEnumerator );
            if ( null == _serverEnumerator ) {
                _serverEnumerator = Servers.GetEnumerator( );
            }

            Debug.Print( "NetworkWrapper.Connect: advancing enumerator" );
            if ( !_serverEnumerator.MoveNext( ) ) {
                Debug.Print( "NetworkWrapper.Connect: out of server:port pairs to try." );
                CleanUp( );
                return;
            }
            _currentServer = _serverEnumerator.Current;
            OnServerChange( _currentServer );

            Debug.Print( "NetworkWrapper.Connect: Calling _currentServer.Connect()" );
            try {
                _currentServer.Connect( );
            }
            catch ( Exception e ) {
                Debug.Print( "NetworkWrapper.Connect: _currentServer.Connect() threw an exception:\n{0}", e );
            }
        }

        public void Disconnect( ) {
            if ( !IsConnected ) {
                Debug.Print( "NetworkWrapper.Disconnect: not connected" );
                return;
            }

            IsConnected = false;
            CleanUp( );
        }

        private void CleanUp( ) {
            Debug.Print( "NetworkWrapper.CleanUp()" );
            _currentServer = null;
            _serverEnumerator = null;
            _haveRaisedStartedEvent = false;
        }

        private void HandleConnectFailed( object sender, EventArgs ev ) {
            Debug.Print( "NetworkWrapper.HandleConnectFailed" );
            OnConnectFailed( );
        }

        private void HandleConnectStarted( object sender, EventArgs ev ) {
            Debug.Print( "NetworkWrapper.HandleConnectStarted" );
            if ( !_haveRaisedStartedEvent ) {
                _haveRaisedStartedEvent = true;
                OnConnectStarted( );
            }
        }

        private void HandleConnectSucceeded( object sender, EventArgs ev ) {
            Debug.Print( "NetworkWrapper.HandleConnectSucceeded" );
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

        private void OnServerChange( ServerWrapper newServerHostName ) {
            var handler = ServerChange;
            if ( null != handler ) {
                handler( this, new ServerChangeEventArgs( newServerHostName ) );
            }
        }

    }

}
