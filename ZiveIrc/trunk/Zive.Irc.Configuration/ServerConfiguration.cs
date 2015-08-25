using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class ServerRef {

        public static implicit operator ServerConfiguration( ServerRef networkRef ) {
            return RefIdManager.GetObject( networkRef.RefId ) as ServerConfiguration;
        }

        public int RefId { get; set; }

    }

    [XmlType]
    public class ServerConfiguration {

        public ServerConfiguration( ) {
            Ports = new Collection<int>( );
            RefId = RefIdManager.GetNext( );
        }

        public static implicit operator ServerRef( ServerConfiguration server ) {
            return new ServerRef { RefId = server._refId };
        }

        public string Name { get; set; }

        private int _refId;
        public int RefId {
            get { return _refId; }
            set {
                if ( _refId == value ) {
                    return;
                }

                if ( _refId > 0 ) {
                    RefIdManager.Unregister( _refId );
                }
                _refId = value;
                if ( _refId > 0 ) {
                    RefIdManager.Register( _refId, this );
                }
            }
        }

        public string Description { get; set; }

        public NetworkRef Network { get; set; }

        public string HostName { get; set; }

        [XmlArray]
        [XmlArrayItem( typeof( int ), ElementName = "Port" )]
        public Collection<int> Ports { get; set; }

    }

}
