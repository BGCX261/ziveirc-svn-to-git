using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

using Zive.Irc.Utility;

namespace Zive.Irc.IdentService {

    public class IdentHandler {

        private readonly static Regex IdentRequestMatcher = new Regex( @"^\s*(\d+)\s*,\s*(\d+)\s*?\r\n$" );

        private readonly FancyBuffer _buffer = new FancyBuffer( 4096 );
        private readonly NetworkStream _stream;
        private readonly TcpClient _tcpClient;
        private string _leftOverPartialLineBuffer = string.Empty;

        public IdentHandler( TcpClient tcpClient ) {
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream( );
        }

        public void Start( ) {
            Debug.Print( "IdentHandler.Start" );
            _StartReading( );
        }

        private void _StartReading( ) {
            Debug.Print( "IdentHandler._StartReading" );
            try {
                _buffer.BeginRead( _stream, _ReadCallback );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer._AcceptTcpClientCallback: _stream.BeginRead threw exception:\n{0}", e );
                _stream.Close( );
                _tcpClient.Close( );
            }
        }

        private readonly string[] _lineSeparator = new[ ] {
            "\r\n"
        };

        private void _ReadCallback( IAsyncResult ar ) {
            Debug.Print( "IdentHandler._ReadCallback" );

            int rc;

            try {
                rc = _buffer.EndRead( ar );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer._ReadCallback: _buffer.EndRead threw exception:\n{0}", e );
                _stream.Close( );
                _tcpClient.Close( );
                return;
            }
            if ( 0 == rc ) {
                Debug.Print( "IdentServer._ReadCallback: connection closed" );
                _stream.Close( );
                _tcpClient.Close( );
                return;
            }
            _buffer.Offset += rc;

            string temp = _leftOverPartialLineBuffer + new string( Encoding.UTF8.GetChars( _buffer.Buffer, 0, _buffer.Offset ) );
            _buffer.Clear( );
            var endOfLine = temp.IndexOf( "\r\n", StringComparison.Ordinal );
            string request;
            if ( endOfLine > -1 ) {
                request = temp.Substring( 0, endOfLine + 2 );
            } else {
                _leftOverPartialLineBuffer = temp;
                _StartReading( );
                return;
            }

            string response;
            try {
                response = _HandleRequest( request );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentHandler._ReadCallback: _HandleRequest threw exception:\n{0}", e );
                _stream.Close( );
                _tcpClient.Close( );
                return;
            }

            try {
                var buf = Encoding.UTF8.GetBytes( response );
                _stream.BeginWrite( buf, 0, buf.Length, _WriteCallback, null );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentHandler._ReadCallback: BeginWrite threw exception:\n{0}", e );
                _stream.Close( );
                _tcpClient.Close( );
            }
        }

        private void _WriteCallback( IAsyncResult ar ) {
            Debug.Print( "IdentHandler._WriteCallback( )" );
            try {
                _stream.EndWrite( ar );
            }
            catch ( Exception e ) {
                Debug.Print( "IdentServer._WriteCallback: _buffer.EndWrite threw exception:\n{0}", e );
            }
            _stream.Close( );
            _tcpClient.Close( );
        }

        private string _HandleRequest( string request ) {
            Debug.Print( "_HandleRequest: Request '{0}'", request.Escape( ) );

            var m = IdentRequestMatcher.Match( request );
            if ( !m.Success ) {
                Debug.Print( "_HandleRequest: Regex matcher failed." );
                throw new InvalidOperationException( "Regex matcher failed." );
            }

            int ourPort;
            int theirPort;
            if ( !int.TryParse( m.Groups[ 1 ].Value, out ourPort ) ) {
                var msg = string.Format( "Our port: Can't interpret '{0}' as an integer.", m.Groups[ 1 ].Value );
                Debug.Print( "_HandleRequest: " + msg );
                throw new InvalidOperationException( msg );
            }
            if ( !int.TryParse( m.Groups[ 2 ].Value, out theirPort ) ) {
                var msg = string.Format( "Their port: Can't interpret '{0}' as an integer.", m.Groups[ 2 ].Value );
                Debug.Print( "_HandleRequest: " + msg );
                throw new InvalidOperationException( msg );
            }
            Debug.Print( "_HandleRequest: Our port {0}; their port {1}", ourPort, theirPort );

            string response = string.Format( "{0}, {1} : ", ourPort, theirPort );
            if ( SocketRegistry.LookUp( ourPort, theirPort ) ) {
                response += string.Format( "USERID : UNIX : {0}\r\n", "foo" );
            } else {
                response += "ERROR : UNKNOWN-ERROR\r\n";
            }

            Debug.Print( "_HandleRequest: response: '{0}'", response.Escape( ) );
            return response;
        }

    }

}
