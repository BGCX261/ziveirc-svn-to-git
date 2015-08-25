using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Zive.Irc.IdentService;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class ServerConnector: IDisposable {

        //
        // Interface
        //

        // Properties

        // Events

        public event EventHandler<ConnectionAttemptStartedEventArgs> ConnectionAttemptStarted;
        public event EventHandler<ConnectionAttemptFailedEventArgs> ConnectionAttemptFailed;
        public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;
        public event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;

        // Constructors

        public ServerConnector( ) {
        }

        public ServerConnector( ConnectionConfiguration connConfig ): this( ) {
            Configuration = connConfig;
        }

        // Methods

        public void BeginConnect( ) {
            if ( string.IsNullOrWhiteSpace( Configuration.ServerHostName ) ) {
                throw new InvalidOperationException( "Configuration problem: ServerHostName not set." );
            }
            if ( Configuration.Ports.Count < 1 ) {
                throw new InvalidOperationException( "Configuration problem: Ports collection is empty." );
            }

            try {
                Dns.BeginGetHostAddresses( Configuration.ServerHostName, DnsGetHostAddressesCallback, null );
            }
            catch ( ArgumentOutOfRangeException e ) {
                Debug.Print( "ServerConnector.BeginConnect: Dns.BeginGetHostAddresses threw ArgumentOutOfRangeException:\n{0}", e );
                OnConnectionFailed( new InvalidOperationException( "DNS lookup failed: ServerHostName is too long.", e ) );
                return;
            }
            catch ( ArgumentException e ) {
                Debug.Print( "ServerConnector.BeginConnect: Dns.BeginGetHostAddresses threw ArgumentException:\n{0}", e );
                OnConnectionFailed( new InvalidOperationException( "DNS lookup failed: ServerHostName is an invalid host name or IP address.", e ) );
                return;
            }
            catch ( SocketException e ) {
                Debug.Print( "ServerConnector.BeginConnect: Dns.BeginGetHostAddresses threw SocketException:\n{0}", e );
                OnConnectionFailed( new InvalidOperationException( "DNS lookup failed: Socket error occurred while resolving host name.", e ) );
                return;
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.BeginConnect: Dns.BeginGetHostAddresses threw unexpected exception:\n{0}", e );
                OnConnectionFailed( e );
                return;
            }
        }

        //
        // Implementation
        //

        protected static readonly BooleanSwitch ServerConnectorNoisySsl = new BooleanSwitch( "ServerConnectorNoisySsl", "Controls whether ServerConnector generates copious debugging output about SSL.", "false" );

        protected IPAddress[] IPAddresses;
        protected IEnumerable<SslEndPoint> EndPoints;
        protected IEnumerator<SslEndPoint> EndPointEnumerator;
        protected SslEndPoint CurrentEndPoint;
        protected TcpClient TcpClient;
        protected X509CertificateCollection CertificateCollection = new X509CertificateCollection( );
        protected ConnectionConfiguration Configuration;

        private ProtocolHandler _protocolHandler;
        private SslStream _sslStream;
        private Server _server;

        protected void DnsGetHostAddressesCallback( IAsyncResult ar ) {
            try {
                IPAddresses = Dns.EndGetHostAddresses( ar );
            }
            catch ( SocketException e ) {
                Debug.Print( "ServerConnector.DnsGetHostAddressesCallback: Dns.EndGetHostAddresses threw SocketException:\n{0}", e );
                OnConnectionFailed( new InvalidOperationException( "DNS lookup failed: Socket error occurred while resolving host name.", e ) );
                return;
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.DnsGetHostAddressesCallback: Dns.EndGetHostAddresses threw unexpected exception:\n{0}", e );
                OnConnectionFailed( e );
                return;
            }

            try {
                EndPoints =
                    from ipAddress in IPAddresses
                    from port in Configuration.Ports
                    select new SslEndPoint( ipAddress, Math.Abs( port ), ( port < 0 ) );
                EndPointEnumerator = EndPoints.GetEnumerator( );

                TryEstablishConnection( );
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.DnsGetHostAddressesCallback: caught exception:\n{0}", e );
            }
        }

        private void TryEstablishConnection( ) {
            if ( null == TcpClient ) {
                TcpClient = new TcpClient( );
            }

            while ( EndPointEnumerator.MoveNext( ) ) {
                CurrentEndPoint = EndPointEnumerator.Current;
                Debug.Print( "ServerConnector.TryEstablishConnection: trying {0}", CurrentEndPoint );
                OnConnectionAttemptStarted( CurrentEndPoint );

                try {
                    TcpClient.BeginConnect( CurrentEndPoint.Address, CurrentEndPoint.Port, ConnectCallback, null );
                    return;
                }
                catch ( NotSupportedException e ) {
                    Debug.Print( "ServerConnector.TryEstablishConnection: TcpClient.BeginConnect: caught NotSupportedException:\n{0}", e );
                }
                catch ( Exception e ) {
                    Debug.Print( "ServerConnector.TryEstablishConnection: TcpClient.BeginConnect: caught exception:\n{0}", e );
                }
            }

            if ( null != TcpClient ) {
                SocketRegistry.Unregister( TcpClient.Client.LocalEndPoint as IPEndPoint );
                TcpClient.Close( );
                TcpClient = null;
            }

            EndPointEnumerator.Dispose( );
            EndPoints = null;

            OnConnectionFailed( new Exception( "Ran out of IP addresses and ports to try." ) );
        }

        private void ConnectCallback( IAsyncResult ar ) {
            bool retry = false;
            try {
                TcpClient.EndConnect( ar );
            }
            catch ( SocketException e ) {
                Debug.Print( "ServerConnector.ConnectCallback: TcpClient.EndConnect: caught SocketException, error code {0}:\n{1}", e.ErrorCode, e );
                OnConnectionAttemptFailed( CurrentEndPoint, e );
                retry = true;
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.ConnectCallback: TcpClient.EndConnect: caught exception:\n{0}", e );
                OnConnectionAttemptFailed( CurrentEndPoint, e );
                retry = true;
            }

            if ( retry ) {
                TryEstablishConnection( );
                return;
            }

            EndPointEnumerator.Dispose( );
            EndPoints = null;

            Debug.Print( "ServerConnector.ConnectCallback: Connected!" );
            SocketRegistry.Register( TcpClient.Client.LocalEndPoint as IPEndPoint, TcpClient.Client.RemoteEndPoint as IPEndPoint );

            Debug.Print( "ServerConnector.ConnectCallback: Constructing objects." );

            var selfUser = new SelfUser {
                NickName = Configuration.NickName,
                HostName = Configuration.LocalHostName,
                RealHostName = Configuration.LocalHostName,
                RealName = Configuration.RealName,
                UserName = Configuration.UserName,
                NickServUserName = Configuration.NickServUserName,
                NickServPassword = Configuration.NickServPassword,
            };

            _server = new Server {
                ServerEndPoint = CurrentEndPoint,
                SelfUser = selfUser,
                ServerHostName = Configuration.ServerHostName,
                ServerPassword = Configuration.Password,
            };
            _protocolHandler = new ProtocolHandler {
                TcpClient = TcpClient,
                SelfUser = selfUser,
                Server = _server,
            };
            _server.ProtocolHandler = _protocolHandler;

            selfUser.Server = _server;

            if ( CurrentEndPoint.UseSsl ) {
                Debug.Print( "ServerConnector.ConnectCallback: Starting SSL." );
                _sslStream = new SslStream( TcpClient.GetStream( ), true, ServerCertificateValidationCallback, LocalCertificateSelectionCallback, EncryptionPolicy.RequireEncryption );
                try {
                    _sslStream.BeginAuthenticateAsClient( Configuration.ServerHostName, CertificateCollection, SslProtocols.Default, true, AuthenticateAsClientCallback, null );
                }
                catch ( Exception e ) {
                    Debug.Print( "ServerConnector.ConnectCallback: Caught exception calling BeginAuthenticateAsClient:\n{0}", e );
                    throw;
                }
            } else {
                FinishConnection( TcpClient.GetStream( ) );
            }
        }

        private bool ServerCertificateValidationCallback( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors ) {
            Debug.Print( "ServerConnector.ServerCertificateValidationCallback" );

            if ( ServerConnectorNoisySsl.Enabled ) {
                Debug.Print( "+ Certificate:\n{0}", certificate );
                Debug.Print( "+ Chain:             {0}", chain );
                Debug.Print( "+ SSL policy errors: {0}", sslPolicyErrors );
                Debug.Print( "+ Chain status:      {0}", string.Join( ", ", chain.ChainStatus.Select( s => s.Status ) ) );
            }
#if !DEBUG
            if ( sslPolicyErrors != SslPolicyErrors.None )
                return false;
#endif
            return true;
        }

        private X509Certificate LocalCertificateSelectionCallback( object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[ ] acceptableIssuers ) {
            Debug.Print( "ServerConnector.LocalCertificateSelectionCallback: target host '{0}'", targetHost );
            if ( ServerConnectorNoisySsl.Enabled ) {
                Debug.Print( "+ Local certificates: {0}", localCertificates );
                Debug.Print( "+ Remote certificate: {0}", remoteCertificate );
                Debug.Print( "+ Acceptable issuers:" );
                foreach ( var issuer in acceptableIssuers ) {
                    Debug.Print( "+ {0}", issuer );
                }
            }

            return null;
        }

        private void AuthenticateAsClientCallback( IAsyncResult ar ) {
            bool retry = false;
            try {
                _sslStream.EndAuthenticateAsClient( ar );
            }
            catch ( IOException e ) {
                Debug.Print( "ServerConnector.AuthenticateAsClientCallback: Caught I/O exception calling EndAuthenticateAsClient, HResult=0x{1:X}:\n{0}", e, e.GetHResult( ) );
                retry = true;
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.AuthenticateAsClientCallback: Caught exception calling EndAuthenticateAsClient:\n{0}", e );
                return;
            }

            if ( retry ) {
                TryEstablishConnection( );
                return;
            }

            if ( ServerConnectorNoisySsl.Enabled ) {
                Debug.Print( "ServerConnector.AuthenticateAsClientCallback: success! interesting properties:" );
                Debug.Print( "+ IsAuthenticated:           {0}", _sslStream.IsAuthenticated );
                Debug.Print( "+ IsEncrypted:               {0}", _sslStream.IsEncrypted );
                Debug.Print( "+ IsMutuallyAuthenticated:   {0}", _sslStream.IsMutuallyAuthenticated );
                Debug.Print( "+ IsServer:                  {0}", _sslStream.IsServer );
                Debug.Print( "+ IsSigned:                  {0}", _sslStream.IsSigned );
                Debug.Print( "+ SslProtocol:               {0}", _sslStream.SslProtocol );
                Debug.Print( "+ CheckCertRevocationStatus: {0}", _sslStream.CheckCertRevocationStatus );
                Debug.Print( "+ CipherAlgorithm:           {0}", _sslStream.CipherAlgorithm );
                Debug.Print( "+ CipherStrength:            {0}", _sslStream.CipherStrength );
                Debug.Print( "+ HashAlgorithm:             {0}", _sslStream.HashAlgorithm );
                Debug.Print( "+ HashStrength:              {0}", _sslStream.HashStrength );
                Debug.Print( "+ KeyExchangeAlgorithm:      {0}", _sslStream.KeyExchangeAlgorithm );
                Debug.Print( "+ KeyExchangeStrength:       {0}", _sslStream.KeyExchangeStrength );
                Debug.Print( "+ LocalCertificate:          {0}", _sslStream.LocalCertificate );
                Debug.Print( "+ RemoteCertificate:         {0}", _sslStream.RemoteCertificate );
            } else {
                Debug.Print( "ServerConnector.AuthenticateAsClientCallback: success!" );
            }

            FinishConnection( _sslStream );
            _sslStream = null;
        }

        private void FinishConnection( Stream stream ) {
            Debug.Print( "ServerConnector.FinishConnection" );
            try {
                _protocolHandler.ConnectionReady( stream );
                _protocolHandler = null;
                TcpClient = null;

                var server = _server;
                _server = null;
                OnConnectionEstablished( server );
            }
            catch ( Exception e ) {
                Debug.Print( "ServerConnector.FinishConnection: caught exception:\n{0}", e );
            }
        }

        // Event dispatchers

        protected virtual void OnConnectionAttemptStarted( SslEndPoint sslEndPoint ) {
            var handler = ConnectionAttemptStarted;
            if ( null != handler ) {
                handler( this, new ConnectionAttemptStartedEventArgs( sslEndPoint ) );
            }
        }

        protected virtual void OnConnectionAttemptFailed( SslEndPoint sslEndPoint, Exception e ) {
            var handler = ConnectionAttemptFailed;
            if ( null != handler ) {
                handler( this, new ConnectionAttemptFailedEventArgs( sslEndPoint, e ) );
            }
        }

        protected virtual void OnConnectionFailed( Exception e ) {
            var handler = ConnectionFailed;
            if ( null != handler ) {
                handler( this, new ConnectionFailedEventArgs( e ) );
            }
        }

        protected virtual void OnConnectionEstablished( Server server ) {
            var handler = ConnectionEstablished;
            if ( null != handler ) {
                handler( this, new ConnectionEstablishedEventArgs( server ) );
            }
        }

        //
        // IDisposable implementation
        //

        ~ServerConnector( ) {
            Dispose( false );
        }

        public void Dispose( ) {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing ) {
            if ( disposing ) {
                ConnectionAttemptStarted = null;
                ConnectionAttemptFailed = null;
                ConnectionFailed = null;
                ConnectionEstablished = null;

                EndPoints = null;
                IPAddresses = null;
                CurrentEndPoint = null;
                CertificateCollection = null;
                Configuration = null;

                if ( null != EndPointEnumerator ) {
                    EndPointEnumerator.Dispose( );
                    EndPointEnumerator = null;
                }
                if ( null != TcpClient ) {
                    TcpClient.Close( );
                    TcpClient = null;
                }
                if ( null != _protocolHandler ) {
                    _protocolHandler.Dispose( );
                    _protocolHandler = null;
                }
                if ( null != _sslStream ) {
                    _sslStream.Dispose( );
                    _sslStream = null;
                }
            }


        }

    }

}
