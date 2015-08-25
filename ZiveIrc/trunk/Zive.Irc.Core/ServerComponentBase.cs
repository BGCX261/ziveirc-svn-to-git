using System;
using System.Collections.Generic;

namespace Zive.Irc.Core {

    public abstract class ServerComponentBase: IServerComponent {

        //
        // Interface
        //

        public Server Server { get; set; }

        protected Dictionary<string, EventHandler<MessageEventArgs>> VerbsToRegister;

        public void RegisterMessages( ) {
            var md = Server.ProtocolHandler.MessageDiscriminator;
            foreach ( var item in VerbsToRegister ) {
                md[ item.Key ] += item.Value;
            }
        }

        public void UnregisterMessages( ) {
            var md = Server.ProtocolHandler.MessageDiscriminator;
            foreach ( var item in VerbsToRegister ) {
                md[ item.Key ] -= item.Value;
            }
        }

    }

}
