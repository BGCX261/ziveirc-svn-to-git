using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class ChannelConfiguration {

        public ChannelConfiguration( ) {
            Networks = new Collection<NetworkRef>( );
            Servers = new Collection<ServerRef>( );
        }

        public string Name { get; set; }

        public string Description { get; set; }

        [XmlArray]
        [XmlArrayItem( typeof( NetworkRef ) )]
        public Collection<NetworkRef> Networks { get; set; }

        [XmlArray]
        [XmlArrayItem( typeof( ServerRef ) )]
        public Collection<ServerRef> Servers { get; set; }

    }

}
