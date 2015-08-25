#define IGNORE_COMMAND_LINE_ARGS
#define USE_BELAY
#undef  USE_VIOLET

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Zive.Irc.Core;
using Zive.Irc.IdentService;
using Zive.Irc.Utility;

//
// Debug command-line arguments:
// "belay.foonetic.net +7001,+6697,7000,6669,6668,6667"
// "violet.zive.ca +6698,6668"
//

namespace Zive.Irc.ConsoleHarness {

    class Program: IDisposable {

        private IdentServer _identServer;

        private ServerConnector _serverConnector;
        private ProtocolHandler _protocolHandler;
        private Server _server;
        private SelfUser _selfUser;
        private bool _connected;

        static void Main( string[ ] args ) {
#if IGNORE_COMMAND_LINE_ARGS
#if USE_BELAY && USE_VIOLET
#error Select a SINGLE set of replacement command-line arguments to use.
#endif
#if USE_BELAY
            args = new[ ] {
                "belay.foonetic.net",
                "+7001,+6697,7000,6669,6668,6667",
            };
#elif USE_VIOLET
            args = new[ ] {
                "violet.zive.ca",
                "+6698,6668",
            };
#else
#error Select a set of replacement command-line arguments to use.
#endif
#endif

            if ( args.Length < 2 ) {
                Console.Error.WriteLine( "Usage: ConsoleHarness <hostName> <port<,port...>>" );
                Console.Error.WriteLine( "Prefix port numbers with a '+' to indicate SSL." );
                return;
            }

            var program = new Program( );
            program.Run( args );
            program.Dispose( );
        }

        private ManualResetEvent _doneEvent = new ManualResetEvent( false );

        private void Run( string[ ] args ) {
#if DEBUG
            Debug.Listeners.Add( new CustomTraceListener( ) );
#endif

            var ports = new Collection<int>( );
            foreach ( var arg in args[ 1 ].Split( new[ ] { ',' }, StringSplitOptions.RemoveEmptyEntries ) ) {
                if ( '+' == arg[ 0 ] ) {
                    IntHelper.TryParse( arg.Substring( 1 ), _ => ports.Add( -_ ) );
                } else {
                    IntHelper.TryParse( arg, ports.Add );
                }
            }

            Console.CancelKeyPress += ConsoleCancelKeyPress;

            _identServer = new IdentServer( );
            _identServer.Start( );

            _serverConnector = new ServerConnector( new ConnectionConfiguration {
                NickName = "ZiveIrcTest",
                UserName = "icekarma",
                RealName = "Testing instance of ZiveIrc. Contact: IceKarma",
                NickServPassword = "timnpfns",
                ServerHostName = args[ 0 ],
                Ports = ports,
            } );

            _serverConnector.ConnectionEstablished += ConnectionEstablished;
            _serverConnector.ConnectionAttemptStarted += ConnectionAttemptStarted;
            _serverConnector.ConnectionAttemptFailed += ConnectionAttemptFailed;
            _serverConnector.ConnectionFailed += ConnectionFailed;

            Console.WriteLine( "Starting connection attempt." );
            _serverConnector.BeginConnect( );

            _doneEvent.WaitOne( );
            _doneEvent.Dispose( );
            _doneEvent = null;
        }

        private void ConsoleCancelKeyPress( object sender, ConsoleCancelEventArgs ev ) {
            Debug.Print( "Program.ConsoleCancelKeyPress: interrupted by {0}", ev.SpecialKey );
            if ( _connected ) {
                Debug.Print( "+ Connected to server: sending QUIT" );
                _server.SendQuit( );
            } else {
                Debug.Print( "+ Not connected to server" );
            }
            ev.Cancel = true;
        }

        private void ConnectionAttemptStarted( object sender, ConnectionAttemptStartedEventArgs ev ) {
            Console.WriteLine( "ConsoleHarness: ConnectionAttemptStarted:" );
            Console.WriteLine( "+ remote end point: {0}", ev.SslEndPoint );
            Console.WriteLine( );
        }

        private void ConnectionAttemptFailed( object sender, ConnectionAttemptFailedEventArgs ev ) {
            Console.WriteLine( "ConsoleHarness: ConnectionAttemptFailed:" );
            Console.WriteLine( "+ remote end point: {0}", ev.SslEndPoint );
            Console.WriteLine( "+ exception:\n{0}", ev.Exception );
            Console.WriteLine( );
        }

        private void ConnectionFailed( object sender, EventArgs ev ) {
            Console.WriteLine( "ConsoleHarness: ConnectionFailed." );
            Console.WriteLine( );

            if ( null != _serverConnector ) {
                _serverConnector.Dispose( );
                _serverConnector = null;
            }

            _doneEvent.Set( );
        }

        private void ConnectionEstablished( object sender, ConnectionEstablishedEventArgs ev ) {
            Console.WriteLine( "ConsoleHarness: ConnectionEstablished:" );
            Console.WriteLine( "+ remote end point: {0}", ev.Server.ServerEndPoint );
            Console.WriteLine( );

            _serverConnector = null;

            _server = ev.Server;
            _protocolHandler = _server.ProtocolHandler;
            _selfUser = _server.SelfUser;

            _selfUser.Error += HandleError;
            _selfUser.Quit += HandleSelfQuit;
            _server.Registered += HandleRegistered;
            _server.MotdComplete += HandleMotdComplete;

            _connected = true;
        }

        private void HandleRegistered( object sender, RegisteredEventArgs ev ) {
            Console.WriteLine( "-- User registration complete, nick is '{0}'. We're ready to go!", ev.NickName );
            _server.ProtocolHandler.SendToServer( "WHOIS {0} {0}", _server.SelfUser.NickName );

            //_server.ProtocolHandler.SendToServer( "PRIVMSG {0} :testing, testing, one two three!", _server.SelfUser.NickName );
            //_server.ProtocolHandler.SendToServer( "PRIVMSG {0} :ⓤⓝⓘⓒⓞⓓⓔⓊⓃⒾⒸⓄⒹⒺ♪☺╔∙☺♫☼æþâçß£öªðõ¡¿é×字☺⍥⪘Ⱍ‽✡✂✌☬☤☣☢☩☁☀☂☥☸☹☺☻♂♀✈✇✆℀℁ℂ℃℄℅℆ℇ℈℉ℊℋℌℍℎℏℐℑℒℓ℔ℕ№℗℘ℙℚℛℜℝ℞℟℠℡™℣ℤ℥Ω℧ℨ℩KÅℬℭ℮ℯℰℱℲℳℴℵℶℷℸℹ℺℻ℼℽℾℿ⅀⅁⅂⅃⅄ⅅⅆⅇⅈⅉ⅊⅋⅌⅍ⅎ⅏µniçø∂´e iß åweßømeuʍop-ǝpı̣sdn unɟ ǝɹoɯ sı̣ ǝpoɔı̣un๑۩۞۩๑㋛Ｕｎｉｃｏｄｅ⦑Bacon!⦒Ａｗｅｓｏｍｅ“smartened” quotes, real dashes (–, —) and currency (£, ¥, ¢)☃☃☃☃☃☃☃☃*【•】_【•】*☢ ☤ ☣ ‼ ⁂ ‽ ℣ ☃ ☮ ☯ ☪ ☭ Ⓦⓘⓝⓝⓘⓝⓖ ⓐⓣ Ⓤⓝⓒⓘⓞⓓⓔ𝔘𝔫𝔦𝔠𝔬𝔡𝔢ユニコードです。涼宮ハルヒの消失𝔹𝕌𝕋𝕋𝕊䷆䷚䷳䷎", _server.SelfUser.NickName );
        }

        private void HandleMotdComplete( object sender, MotdCompleteEventArgs motdCompleteEventArgs ) {
            _server.ProtocolHandler.SendToServer( "JOIN #ziveirc-testing" );
        }

        private void HandleError( object sender, MessageEventArgs ev ) {
            Console.WriteLine( "-- Received ERROR" );

            _doneEvent.Set( );
        }

        private void HandleSelfQuit( object sender, MessageEventArgs ev ) {
            Console.WriteLine( "-- Received QUIT for self" );

            _doneEvent.Set( );
        }

        //
        // IDispose implementation
        //

        ~Program( ) {
            Dispose( false );
        }

        public void Dispose( ) {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing ) {
            if ( disposing ) {
                if ( null != _serverConnector ) {
                    _serverConnector.Dispose( );
                    _serverConnector = null;
                }
                if ( null != _doneEvent ) {
                    _doneEvent.Dispose( );
                    _doneEvent = null;
                }
            }
        }

    }

}
