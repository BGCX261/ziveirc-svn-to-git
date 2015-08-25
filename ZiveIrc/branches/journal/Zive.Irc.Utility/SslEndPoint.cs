using System.Net;

namespace Zive.Irc.Utility {

    public class SslEndPoint: IPEndPoint {

        public SslEndPoint( IPAddress ipAddress, int port, bool useSsl ): base( ipAddress, port ) {
            UseSsl = useSsl;
        }

        public SslEndPoint( long ipAddress, int port, bool useSsl ): base( ipAddress, port ) {
            UseSsl = useSsl;
        }

        public bool UseSsl { get; set; }

        public override string ToString( ) {
            return Address + ( UseSsl ? ":+" : ":" ) + Port;
        }

    }

}
