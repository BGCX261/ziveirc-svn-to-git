using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class ProtocolHandler: IDisposable {

        //
        // Interface
        //

        // Properties

        public SelfUser SelfUser {
            get { return _selfUser; }
            set { _selfUser = value; }
        }
        public Server Server {
            get { return _server; }
            set { _server = value; }
        }
        public TcpClient TcpClient {
            get { return _tcpClient; }
            set { _tcpClient = value; }
        }
        public Stream Stream {
            get { return _stream; }
            set { _stream = value; }
        }

        // Fields

        public MessageDiscriminator MessageDiscriminator = new MessageDiscriminator( );

        // Constants

        public const int IrcLineLengthLimit = 510;

        // Constructors

        // Methods

        public void SendToServer( string line, params object[ ] args ) {
            var stringToSend = string.Format( line, args );
            Debug.Print( "< " + stringToSend );
            var charCount = stringToSend.Length > IrcLineLengthLimit ? IrcLineLengthLimit : stringToSend.Length;
            var charsToSend = new char[ IrcLineLengthLimit + 2 ]; // add room for trailing CRLF
            stringToSend.CopyTo( 0, charsToSend, 0, charCount );

            // Loop until we've shortened the string to IrcLineLengthLimit characters or less
            int length = Encoding.UTF8.GetByteCount( charsToSend, 0, charCount );
            while ( length > IrcLineLengthLimit ) {
                charCount--;
                length = Encoding.UTF8.GetByteCount( charsToSend, 0, charCount );
            }

            charsToSend[ charCount++ ] = '\r';
            charsToSend[ charCount++ ] = '\n';
            byte[] bytesToSend = Encoding.UTF8.GetBytes( charsToSend, 0, charCount );

            try {
                Stream.BeginWrite( bytesToSend, 0, bytesToSend.Length, _WriteCallback, null );
            }
            catch ( Exception e ) {
                Debug.Print( "ProtocolHandler._SendToServer: BeginWrite threw exception:\n{0}", e );
                throw;
            }
        }

        public void ConnectionReady( Stream stream ) {
            Stream = stream;
            if ( null == _buffer ) {
                _buffer = new FancyBuffer( TcpClient.Client.ReceiveBufferSize * 2 );
            }
            _StartRead( );
            Server.SendUserRegistration( );
        }

        public void SaveAndClearHandlers( ) {
            _stack.Push( MessageDiscriminator );
            MessageDiscriminator = new MessageDiscriminator( );
        }

        public void RestoreHandlers( ) {
            MessageDiscriminator = _stack.Pop( );
        }

        //
        // Implementation
        //

        // Property backing fields

        private SelfUser _selfUser;
        private Server _server;
        private TcpClient _tcpClient;
        private Stream _stream;

        // Other fields

        private static readonly BooleanSwitch ShowRawServerInput = new BooleanSwitch( "ShowRawServerInput", "Controls whether raw input from the server is emitted to the debugging log.", "0" );

        private string _leftOverPartialLineBuffer = string.Empty;
        private FancyBuffer _buffer;
        private readonly Stack<MessageDiscriminator> _stack = new Stack<MessageDiscriminator>( );

        // Methods

        private void _StartRead( ) {
            try {
                _buffer.BeginRead( Stream, _ReadCallback );
            }
            catch ( Exception e ) {
                Debug.Print( "ProtocolHandler._StartRead: BeginRead threw exception:\n{0}", e );
                throw;
            }
        }

        private void _ReadCallback( IAsyncResult ar ) {
            int rc;

            try {
                rc = _buffer.EndRead( ar );
            }
            catch ( Exception e ) {
                Debug.Print( "ProtocolHandler._ReadCallback: EndRead threw exception:\n{0}", e );
                throw;
            }
            _buffer.Offset += rc;

            try {
                string temp = _leftOverPartialLineBuffer + new string( Encoding.UTF8.GetChars( _buffer.Buffer, 0, _buffer.Offset ) );
                _buffer.Clear( );
                List<string> lines = temp.Split( CommonDelimiters.CrLf, StringSplitOptions.None ).ToList( );
                int lastLine = lines.Count - 1;
                _leftOverPartialLineBuffer = lines[ lastLine ];
                lines.RemoveAt( lastLine );

                foreach ( var line in lines ) {
                    if ( ShowRawServerInput.Enabled ) {
                        Debug.Print( "> " + line );
                    }
                    OnMessage( new Message( Server, line ) );
                }

                _StartRead( );
            }
            catch ( Exception e ) {
                Debug.Print( "ProtocolHandler._ReadCallback: caught exception while processing server input:\n{0}", e );
            }
        }

        private void _WriteCallback( IAsyncResult ar ) {
            try {
                Stream.EndWrite( ar );
            }
            catch ( Exception e ) {
                Debug.Print( "ProtocolHandler._WriteCallback: EndWrite threw exception:\n{0}", e );
                throw;
            }
        }

        protected virtual void OnMessage( Message message ) {
            var md = MessageDiscriminator;
            var handler = md[ message ] ?? md[ string.Empty ];
            if ( null != handler ) {
                handler( this, new MessageEventArgs( message ) );
            }
        }

        //
        // IDisposable
        //

        ~ProtocolHandler( ) {
            Dispose( false );
        }

        public void Dispose( ) {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing ) {
            if ( disposing ) {
                if ( null != Stream ) {
                    Stream.Dispose( );
                    Stream = null;
                }
                if ( null != TcpClient ) {
                    TcpClient.Close( );
                    TcpClient = null;
                }
            }
        }

    }

}
