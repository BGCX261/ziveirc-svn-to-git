using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class WhoResponseParser {

        public Server Server { get; set; }
        public ProtocolHandler ProtocolHandler { get; set; }

        public virtual Collection<WhoResponse> ParseCollection( Collection<Message> messages ) {
            var result = new Collection<WhoResponse>( );
            foreach ( var message in messages ) {
                result.Add( ParseResponse( message ) );
            }
            return result;
        }

        public virtual WhoResponse ParseResponse( Message message ) {
            var channel = Server.LookUpChannel( message.Args[ 0 ] );

            var realName = message.Args[ 6 ];
            var index = realName.IndexOf( ' ' );
            int hopCount = -1;
            if ( -1 != index ) {
                if ( Int32.TryParse( realName.Substring( 0, index ), NumberStyles.Integer, CultureInfo.InvariantCulture, out hopCount ) ) {
                    realName = realName.Substring( index + 1 );
                }
            }

            var response = new WhoResponse {
                ChannelName = message.Args[ 0 ],
                UserName = message.Args[ 1 ],
                HostName = message.Args[ 2 ],
                ServerName = message.Args[ 3 ],
                NickName = message.Args[ 4 ],
                Flags = message.Args[ 5 ],
                HopCount = hopCount,
                RealName = realName,
            };

            var user = Server.LookUpUser( response.NickName ) ?? new User {
                NickName = response.NickName,
                ProtocolHandler = ProtocolHandler,
                Server = Server,
            };
            user.UserName = response.UserName;
            user.HostName = response.HostName;
            user.RealName = response.RealName;

            if ( null != channel ) {
                try {
                    user.Channels.AddIfMissing( message.Args[0], channel );
                    channel.Users.Add( user );
                }
                catch ( Exception e ) {
                    Debug.Print( "WhoResponseParser.ParseResponse: caught exception:\n{0}", e );
                    Debugger.Break( );
                }
            }

            return response;
        }

    }

}
