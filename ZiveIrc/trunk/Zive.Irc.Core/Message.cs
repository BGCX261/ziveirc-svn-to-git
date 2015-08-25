using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zive.Irc.Core {

    public class Message {

        //
        // Interface
        //

        // Properties

        public Server Server { get; set; }
        public string RawLine { get; set; }

        public NickUserHost Origin { get; set; }
        public string Verb { get; set; }
        public string Target { get; set; }
        public List<string> Args { get; set; }

        // Constructors

        public Message( ) {
            RawLine = Verb = Target = string.Empty;
            Args = new List<string>( );
        }

        public Message( Server server, string rawLine ): this( ) {
            Server = server;
            Parse( rawLine );
        }

        // Methods

        public void Parse( string rawLine ) {
            int spaceIndex;

            RawLine = rawLine;
            if ( rawLine[ 0 ] == ':' ) {
                spaceIndex = rawLine.IndexOf( ' ' );
                if ( spaceIndex == -1 ) {
                    Debug.Print( "Message.Parse: Malformed input: starts with an origin but doesn't have a verb or arguments: '{0}'", rawLine );
                    throw new ArgumentException( "Malformed input: starts with an origin but doesn't have a verb or arguments." );
                }
                Origin = new NickUserHost( rawLine.Substring( 1, spaceIndex - 1 ) );
                rawLine = rawLine.Substring( spaceIndex + 1 );
            }

            spaceIndex = rawLine.IndexOf( ' ' );
            int colonIndex = -1;
            while ( -1 != spaceIndex ) {
                if ( rawLine[ 0 ] == ':' ) {
                    colonIndex = Args.Count;
                    Args.Add( rawLine.Substring( 1 ) );
                    rawLine = string.Empty;
                    break;
                }
                string chunk = rawLine.Substring( 0, spaceIndex );
                Args.Add( chunk );
                rawLine = rawLine.Substring( spaceIndex + 1 );
                spaceIndex = rawLine.IndexOf( ' ' );
            }
            if ( !string.IsNullOrWhiteSpace( rawLine ) ) {
                if ( rawLine[ 0 ] == ':' ) {
                    colonIndex = Args.Count;
                    Args.Add( rawLine.Substring( 1 ) );
                } else {
                    Args.Add( rawLine );
                }
            }

            Verb = Args[ 0 ];
            Args.RemoveAt( 0 );
            colonIndex--;

            if ( ( colonIndex > 0 ) || ( colonIndex < 0 && Args.Count > 1 ) ) {
                Target = Args[ 0 ];
                Args.RemoveAt( 0 );
            }
        }

        public override string ToString( ) {
            string result = string.Empty;
            if ( !Origin.IsEmpty ) {
                result = ":" + Origin;
            }
            result += " " + Verb;
            if ( !string.IsNullOrWhiteSpace( Target ) ) {
                result += " " + Target;
            }
            foreach ( var arg in Args ) {
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
