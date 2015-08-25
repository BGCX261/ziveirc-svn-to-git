using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class NamesHandler: ServerComponentBase {

        //
        // Interface
        //

        public ChannelVisibility ChannelVisibility { get; set; }
        public string Target { get; set; }

        public event EventHandler<NamesCompleteEventArgs> Complete;

        public NamesHandler( ) {
            VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "353", HandleRplNamReply },
                { "366", HandleRplEndOfNames },
            };
        }

        // Numeric 353
        //	( '=' / '*' / '@' ) <channel> ' ' : [ '@' / '+' ] <nick> *( ' ' [ '@' / '+' ] <nick> )
        public void HandleRplNamReply( object sender, MessageEventArgs ev ) {
            Target = ev.Message.Args[ 1 ];
            switch ( ev.Message.Args[ 0 ] ) {
                case "=":
                    ChannelVisibility = ChannelVisibility.Public;
                    break;

                case "@":
                    ChannelVisibility = ChannelVisibility.Secret;
                    break;

                case "*":
                    ChannelVisibility = ChannelVisibility.Other;
                    break;

                default:
                    Debug.Print( "NamesHandler.HandleRplNamReply: unknown channel type '{0}'", ev.Message.Args[ 0 ] );
                    break;
            }

            foreach ( var nickName in ev.Message.Args[2].Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries ) ) {
                _nickNames.Add( nickName );
            }
        }

        //
        // Implementation
        //

        private Collection<string> _nickNames = new Collection<string>( );

        // Numeric 366
        private void HandleRplEndOfNames( object sender, MessageEventArgs ev ) {
            OnComplete( _nickNames );
        }

        private void OnComplete( Collection<string> nickNames ) {
            var handler = Complete;
            if ( null != handler ) {
                handler( this, new NamesCompleteEventArgs( nickNames ) );
            }
        }

    }

}
