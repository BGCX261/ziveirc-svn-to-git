using System.Diagnostics;

namespace Zive.Irc.Utility {

    public static class DebugX {

        public static void Print( string format, params object[ ] args ) {
            Trace.Listeners[ 0 ].WriteLine( "***|" + string.Format( format, args ) );
        }

    }

}
