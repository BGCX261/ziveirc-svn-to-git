using System;
using Zive.Irc.Configuration;

namespace Zive.Irc.WpfUi {

    public class ServerWrapper {

        public ServerWrapper( ) {

        }

        public ServerWrapper( ServerConfiguration serverConfiguration ) {
            ServerConfiguration = serverConfiguration;
        }

        public ServerConfiguration ServerConfiguration { get; private set; }
        public string Name { get { return ServerConfiguration.Name; } }
        public int RefId { get { return ServerConfiguration.RefId; } }

        public bool IsConnected { get; set; }

        public void Connect( ) {
            if ( IsConnected ) {
                throw new InvalidOperationException( "Server is already connected" );
            }

            IsConnected = true;
        }

        public void Disconnect( ) {
            if ( !IsConnected ) {
                throw new InvalidOperationException( "Server is not connected" );
            }

            IsConnected = false;
        }

    }

}
