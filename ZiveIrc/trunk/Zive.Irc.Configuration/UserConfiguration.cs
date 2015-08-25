using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class UserConfiguration {

        public UserConfiguration( ) {
            NickName = string.Empty;
            UserName = string.Empty;
            RealName = string.Empty;
        }

        public string NickName;
        public string UserName;
        public string RealName;

        /*
        _serverConnector = new ServerConnector( new ConnectionConfiguration {
            NickServPassword = "timnpfns",
            ServerHostName = _currentServer.ServerConfiguration.HostName,
        } );
        */

    }

}
