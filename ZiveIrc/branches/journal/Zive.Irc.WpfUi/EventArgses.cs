using System;

namespace Zive.Irc.WpfUi {

    public class LineInputEventArgs: EventArgs {

        public LineInputEventArgs( string line ) {
            Line = line;
            AllowClear = true;
        }

        public string Line { get; set; }
        public bool AllowClear { get; set; }

    }

    public class RelayedScrollEventArgs: EventArgs {

        public RelayedScrollEventArgs( ScrollAction scrollAction ) {
            ScrollAction = scrollAction;
        }

        public ScrollAction ScrollAction { get; set; }

    }

    public class ServerChangeEventArgs: EventArgs {

        public ServerChangeEventArgs( ServerWrapper serverWrapper ) {
            ServerWrapper = serverWrapper;
        }

        public ServerWrapper ServerWrapper { get; set; }

    }

}
