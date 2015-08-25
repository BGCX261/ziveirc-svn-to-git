using System;
using System.Collections.Generic;
using System.Net;
using Zive.Irc.Utility;

namespace Zive.Irc.IdentService {

    public static class SocketRegistry {

        private static readonly Dictionary<int, int> Registry = new Dictionary<int, int>( );

        public static void Register( IPEndPoint localEndPoint, IPEndPoint remoteEndPoint ) {
            if ( null == localEndPoint ) {
                throw new ArgumentNullException( "localEndPoint", "Can't be null" );
            }
            if ( null == remoteEndPoint ) {
                throw new ArgumentNullException( "remoteEndPoint", "Can't be null" );
            }

            Registry.Add( localEndPoint.Port, remoteEndPoint.Port );
        }

        public static void Unregister( IPEndPoint localEndPoint ) {
            if ( null == localEndPoint ) {
                return;
            }

            Registry.RemoveIfContains( localEndPoint.Port );
        }

        public static bool LookUp( int ourPort, int theirPort ) {
            return ( ( ourPort > 0 && ourPort < 65536 ) && ( theirPort > 0 && theirPort < 65536 ) && Registry.ContainsKey( ourPort ) && Registry[ ourPort ] == theirPort );
        }

    }

}
