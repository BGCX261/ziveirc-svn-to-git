using System;
using System.Collections.Generic;

namespace Zive.Irc.Core {

    public class Message {

        //
        // Interface
        //

        // Properties

        public Server Server {
            get { return _server; }
        }
        public string RawLine {
            get { return _rawLine; }
        }
        public NickUserHost Origin {
            get { return _origin; }
        }
        public string Verb {
            get { return _verb; }
        }
        public string Target {
            get { return _target; }
        }
        public List<string> Args {
            get { return _args; }
        }

        // Constructors

        public Message( Server server, string rawLine ) {
            _server = server;
            Parse( rawLine );
        }

        // Methods

        public void Parse( string rawLine ) {
            if ( _parsed ) {
                throw new InvalidOperationException( "Object has already been initialized." );
            }
            _parsed = true;

            _rawLine = rawLine;
            int spaceIndex;
            if ( ':' == rawLine[ 0 ] ) {
                spaceIndex = rawLine.IndexOf( ' ' );
                if ( -1 == spaceIndex ) {
                    throw new ArgumentException( "Malformed input: starts with an origin but doesn't have a verb or arguments." );
                }
                _origin.Set( rawLine.Substring( 1, spaceIndex - 1 ) );
                rawLine = rawLine.Substring( spaceIndex + 1 );
            }

            spaceIndex = rawLine.IndexOf( ' ' );
            int colonIndex = -1;
            while ( -1 != spaceIndex ) {
                if ( ':' == rawLine[ 0 ] ) {
                    colonIndex = _args.Count;
                    _args.Add( rawLine.Substring( 1 ) );
                    rawLine = string.Empty;
                    break;
                }
                string chunk = rawLine.Substring( 0, spaceIndex );
                _args.Add( chunk );
                rawLine = rawLine.Substring( spaceIndex + 1 );
                spaceIndex = rawLine.IndexOf( ' ' );
            }
            if ( !string.IsNullOrWhiteSpace( rawLine ) ) {
                if ( ':' == rawLine[ 0 ] ) {
                    colonIndex = _args.Count;
                    _args.Add( rawLine.Substring( 1 ) );
                } else {
                    _args.Add( rawLine );
                }
            }

            _verb = _args[ 0 ];
            _args.RemoveAt( 0 );
            colonIndex--;

            if ( ( colonIndex > 0 ) || ( colonIndex < 0 && _args.Count > 1 ) ) {
                _target = _args[ 0 ];
                _args.RemoveAt( 0 );
            }
        }

        //
        // Implementation
        //

        protected Server _server;
        protected string _rawLine = string.Empty;
        protected NickUserHost _origin = new NickUserHost( );
        protected string _verb = string.Empty;
        protected string _target = string.Empty;
        protected List<string> _args = new List<string>( );

        private bool _parsed;

        //
        // Object
        //

        public override string ToString( ) {
            string result = string.Empty;
            if ( !_origin.IsEmpty ) {
                result = ":" + _origin;
            }
            result += " " + _verb;
            if ( !string.IsNullOrWhiteSpace( _target ) ) {
                result += " " + _target;
            }
            foreach ( var arg in _args ) {
                if ( arg.IndexOf( ' ' ) > -1 ) {
                    result += " :" + arg;
                    break;
                }
                result += " " + arg;
            }
            return result;
        }

    }

}
