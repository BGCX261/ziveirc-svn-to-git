using System;
using System.Globalization;

namespace Zive.Irc.Utility {

    public static class UnixTimestampConverter {
        private static readonly DateTime _unixEpoch  = new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc );
        private static DateTime UnixEpoch {
            get {
                return _unixEpoch;
            }
        }

        public static DateTime FromSeconds( int timeStamp ) {
            return UnixEpoch.AddSeconds( timeStamp );
        }

        public static DateTime FromTicks( long timeStamp ) {
            return UnixEpoch.AddTicks( timeStamp );
        }

        public static DateTime FromStringSeconds( string timeStamp ) {
            return FromSeconds( int.Parse( timeStamp, NumberStyles.Integer ) );
        }

        public static DateTime FromStringTicks( string timeStamp ) {
            return FromTicks( long.Parse( timeStamp, NumberStyles.Integer ) );
        }

        public static long ToSeconds( DateTime dt ) {
            return (long) ( ( dt - UnixEpoch ).TotalSeconds );
        }

        public static long ToTicks( DateTime dt ) {
            return ( dt - UnixEpoch ).Ticks;
        }

        public static string ToStringSeconds( DateTime dt ) {
            return ToSeconds( dt ).ToString( CultureInfo.InvariantCulture );
        }

        public static string ToStringTicks( DateTime dt ) {
            return ToTicks( dt ).ToString( CultureInfo.InvariantCulture );
        }

    }

}
