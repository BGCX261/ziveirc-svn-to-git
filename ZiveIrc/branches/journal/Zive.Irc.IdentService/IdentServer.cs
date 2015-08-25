using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Zive.Irc.IdentService {

    //
    // RFC 1413 Identification Protocol server implementation
    //

    public class IdentServer {

        //
        // Interface
        //

        public void Start( ) {
            Debug.Print( "IdentServer.Start" );
            try {
                _tcpListener = new TcpListener( IPAddress.Any, 113 );
                _tcpListener.Start( );
                _AcceptConnections( );
            }
            catch ( SocketException e ) {
                Debug.Print( "IdentServer.Start: _tcpListener.Start: caught SocketException, error code {0}:\n{1}", e.ErrorCode, e );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer.Start: _tcpListener.Start threw exception:\n{0}", e );
            }
        }

        public void Stop( ) {
            Debug.Print( "IdentServer.Stop" );
            try {
                _tcpListener.Stop( );
            }
            catch ( SocketException e ) {
                Debug.Print( "IdentServer.Start: _tcpListener.Stop: caught SocketException, error code {0}:\n{1}", e.ErrorCode, e );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer.Start: _tcpListener.Stop threw exception:\n{0}", e );
            }
        }

        //
        // Implementation
        //

        private TcpListener _tcpListener;

        private void _AcceptConnections( ) {
            _tcpListener.BeginAcceptTcpClient( _AcceptTcpClientCallback, null );
        }

        private void _AcceptTcpClientCallback( IAsyncResult ar ) {
            TcpClient client;
            try {
                client = _tcpListener.EndAcceptTcpClient( ar );
            }
            catch ( ObjectDisposedException ) {
                Debug.Print( "IdentServer._AcceptTcpClientCallback: _tcpListener.EndAcceptTcpClient threw exception ObjectDisposedException: socket has been closed, terminating" );
                return;
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer._AcceptTcpClientCallback: _tcpListener.EndAcceptTcpClient threw exception:\n{0}", e );
                client = null;
            }

            if ( null != client ) {
                var handler = new IdentHandler( client );
                handler.Start( );
            }

            _AcceptConnections( );
        }

    }

}
