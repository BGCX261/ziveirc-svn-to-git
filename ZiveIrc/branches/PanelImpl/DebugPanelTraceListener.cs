using System.Diagnostics;
using Zive.Irc.Utility;

namespace Zive.Irc.WpfUi {

    public class DebugPanelTraceListener: TraceListener {

        public ScrollbackDebugManager ScrollbackDebugManager;

        public override void Write( string message ) {
            ScrollbackDebugManager.AddLine( message );
        }

        public override void WriteLine( string message ) {
            ScrollbackDebugManager.AddLine( message );
        }

    }

}
