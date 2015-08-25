using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlRoot]
    public class ConfigurationRoot {

        public ConfigurationRoot( ) {
            Networks                = new Collection<NetworkConfiguration>( );
            Servers                 = new Collection<ServerConfiguration>( );
            Channels                = new Collection<ChannelConfiguration>( );
            GeneralConfiguration    = new GeneralConfiguration( );
            MainWindowConfiguration = new MainWindowConfiguration( );
            UserConfiguration       = new UserConfiguration( );
#if DEBUG
            DebuggingConfiguration  = new DebuggingConfiguration( );
#endif
        }

        [XmlArray]
        [XmlArrayItem( typeof( NetworkConfiguration ) )]
        public Collection<NetworkConfiguration> Networks;

        [XmlArray]
        [XmlArrayItem( typeof( ServerConfiguration ) )]
        public Collection<ServerConfiguration> Servers;

        [XmlArray]
        [XmlArrayItem( typeof( ChannelConfiguration ) )]
        public Collection<ChannelConfiguration> Channels;

        public GeneralConfiguration    GeneralConfiguration    { get; set; }
        public MainWindowConfiguration MainWindowConfiguration { get; set; }
        public UserConfiguration       UserConfiguration       { get; set; }
#if DEBUG
        public DebuggingConfiguration  DebuggingConfiguration  { get; set; }
#endif

    }

}
