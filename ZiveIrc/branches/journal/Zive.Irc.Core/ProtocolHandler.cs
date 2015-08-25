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
    using System.Threading;
    using FilterFuncType = Func<Message, bool>;
    using HandlerFuncType = EventHandler<MessageEventArgs>;

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
        public ObservableCollection<Channel> Channels {
            get { return _channels; }
        }
        public ObservableCollection<User> Users {
            get { return _users; }
        }
        public MessageJournal MessageJournal {
            get { return _journal; }
        }

        // Constants

        public const int IrcLineLengthLimit = 510;

        // Constructors

        public ProtocolHandler( ) {
            _journalFilter = new Lazy<MessageJournalFilter>( _MakeJournalFilter, LazyThreadSafetyMode.PublicationOnly );
        }

        private MessageJournalFilter _MakeJournalFilter( ) {
            return new MessageJournalFilter( _journal );
        }

        // Methods

        public void SendToServer( string line, params object[] args ) {
            var stringToSend = string.Format( line, args );
            Debug.Print( "< " + stringToSend );
            var charCount = ( stringToSend.Length > IrcLineLengthLimit ) ? IrcLineLengthLimit : stringToSend.Length;
            var charsToSend = new char[IrcLineLengthLimit + 2]; // include room for trailing CRLF
            stringToSend.CopyTo( 0, charsToSend, 0, charCount );

            // Loop until we've shortened the string to IrcLineLengthLimit characters or less
            var utf8 = Encoding.UTF8;
            int length = utf8.GetByteCount( charsToSend, 0, charCount );
            while ( length > IrcLineLengthLimit ) {
                charCount--;
                length = utf8.GetByteCount( charsToSend, 0, charCount );
            }

            charsToSend[charCount++] = '\r';
            charsToSend[charCount++] = '\n';
            byte[] bytesToSend = utf8.GetBytes( charsToSend, 0, charCount );

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

        public void Subscribe( FilterFuncType filter, HandlerFuncType handler ) {
            _journalFilter.Value.Subscribe( filter, handler );
        }

        public void Subscribe( IList<FilterAndHandler> events ) {
            _journalFilter.Value.Subscribe( events );
        }

        public void Unsubscribe( FilterFuncType filter, HandlerFuncType handler ) {
            _journalFilter.Value.Unsubscribe( filter, handler );
        }

        public void Unsubscribe( IList<FilterAndHandler> events ) {
            _journalFilter.Value.Unsubscribe( events );
        }

        public void StartCapture( string context ) {
            Debug.Print( "ProtocolHandler.StartCapture: context is '{0}'", context );

            Debug.Print( "+ Before clearing and pushing stack:" );
            Debug.Indent( );
            _DumpJournalFilter( );
            Debug.Unindent( );

            _journalFilter.Value.Suspend( );
            _journalFilterStack.Push( new JournalFilterStackEntry( context, _journalFilter ) );
            _journalFilter = new Lazy<MessageJournalFilter>( _MakeJournalFilter, LazyThreadSafetyMode.PublicationOnly );
        }

        public void EndCapture( string context ) {
            Debug.Print( "ProtocolHandler.EndCapture: context is '{0}'", context );

            Debug.Print( "+ Before popping stack:" );
            Debug.Indent( );
            _DumpJournalFilter( );
            Debug.Unindent( );

            // If the stack is empty, goad it into throwing an exception.
            if ( _journalFilterStack.Count < 1 ) {
                _journalFilterStack.Pop( );
            }

            // Peek at the top of the stack, and throw an exception if the context is wrong.
            var stackContext = _journalFilterStack.Peek( ).Context;
            if ( stackContext != context ) {
                throw new InvalidOperationException( string.Format( "Top of stack context '{0}' does not match supplied context '{1}'", stackContext, context ) );
            }

            // Dispose of the _journalFilter we're about to replace
            _journalFilter.Value.Dispose( );
            _journalFilter = null;

            // Restore _journalFilter and pop the stack at the same time.
            _journalFilter = _journalFilterStack.Pop( ).JournalFilter;
            _journalFilter.Value.Resume( );

            Debug.Print( "+ After popping stack:" );
            Debug.Indent( );
            _DumpJournalFilter( );
            Debug.Unindent( );
        }

        private void _DumpJournalFilter( ) {
            Debug.Print( "ProtocolHandler._DumpJournalFilter: _journalFilter's contents:" );
            var filters = _journalFilter.Value.Filters;
            foreach ( var method in filters.Select( _ => _.Handler.Method ) ) {
                Debug.Print( "  + FilteredHandler {0}.{1}", method.DeclaringType.Name, method.Name );
            }
        }

        //
        // Implementation
        //

        // Private classes

        private class JournalFilterStackEntry {

            public JournalFilterStackEntry( string context, Lazy<MessageJournalFilter> journalFilter ) {
                Context = context;
                JournalFilter = journalFilter;
            }

            public string Context;
            public Lazy<MessageJournalFilter> JournalFilter;

        }

        // Property backing fields

        private SelfUser _selfUser;
        private Server _server;
        private TcpClient _tcpClient;
        private Stream _stream;
        private readonly ObservableCollection<Channel> _channels = new ObservableCollection<Channel>( );
        private readonly ObservableCollection<User> _users = new ObservableCollection<User>( );
        private readonly MessageJournal _journal = new MessageJournal( );

        // Other fields

        private static readonly BooleanSwitch ShowRawServerInput = new BooleanSwitch( "ShowRawServerInput", "Controls whether raw input from the server is emitted to the debugging log.", "0" );

        private string _leftOverPartialLineBuffer = string.Empty;
        private FancyBuffer _buffer;

        private readonly Stack<JournalFilterStackEntry> _journalFilterStack = new Stack<JournalFilterStackEntry>( );
        private Lazy<MessageJournalFilter> _journalFilter;

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
                _leftOverPartialLineBuffer = lines[lastLine];
                lines.RemoveAt( lastLine );

                foreach ( var line in lines ) {
                    if ( ShowRawServerInput.Enabled ) {
                        Debug.Print( "> " + line );
                    }

                    var m = new Message( Server, line );
                    _journal.Add( m );
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
