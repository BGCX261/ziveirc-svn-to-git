using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class NetworkRef {

        public static implicit operator NetworkConfiguration( NetworkRef networkRef ) {
            return RefIdManager.GetObject( networkRef.RefId ) as NetworkConfiguration;
        }

        public int RefId { get; set; }

    }

    [XmlType]
    public class NetworkConfiguration {

        public NetworkConfiguration( ) {
            Servers = new Collection<ServerRef>( );
            RefId = RefIdManager.GetNext( );
        }

        public static implicit operator NetworkRef( NetworkConfiguration network ) {
            return new NetworkRef {
                RefId = network._refId
            };
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

        [XmlArray]
        [XmlArrayItem( typeof( ServerRef ) )]
        public Collection<ServerRef> Servers { get; set; }

    }

}
