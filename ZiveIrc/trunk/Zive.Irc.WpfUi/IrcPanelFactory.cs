using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zive.Irc.WpfUi {

    public static class IrcPanelFactory {

        private static readonly Dictionary<string, Type> _panelTypeMap = new Dictionary<string, Type> {
            { "ChannelPanel", typeof( ChannelPanel ) },
            { "ConsolePanel", typeof( ConsolePanel ) },
            { "IrcPanel",     typeof( IrcPanel )     },
            { "QueryPanel",   typeof( QueryPanel )   },
            { "ServerPanel",  typeof( ServerPanel )  },
#if DEBUG
            { "DebugPanel",   typeof( DebugPanel )   },
#endif
        };

        public static IIrcPanel Create( string type ) {
            if ( !_panelTypeMap.ContainsKey( type ) ) {
                Debug.Print( "IrcPanelFactory.Create: Unknown panel type '{0}'", type );
                return null;
            }

            var panelConstructor = _panelTypeMap[ type ].GetConstructor( Type.EmptyTypes );
            if ( null == panelConstructor ) {
                Debug.Print( "IrcPanelFactory.Create: Unable to find constructor for panel type '{0}'", type );
                return null;
            }

            return panelConstructor.Invoke( null ) as IIrcPanel;
        }

    }

}
