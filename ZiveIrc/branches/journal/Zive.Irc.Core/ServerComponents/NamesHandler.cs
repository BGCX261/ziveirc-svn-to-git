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

        public NamesHandler( Server server, EventHandler<NamesCompleteEventArgs> completionHandler, MessageEventArgs ev ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "353" ), HandleRplNamReply   ),
                new FilterAndHandler( ( m ) => ( m.Verb == "366" ), HandleRplEndOfNames ),
            };
            Complete += completionHandler;
            Server = server;
            HandleRplNamReply( null, ev );
        }

        //
        // Implementation
        //

        private Collection<string> _nickNames = new Collection<string>( );

        private static readonly Dictionary<string, ChannelVisibility> ChannelVisibilityStringMap = new Dictionary<string, ChannelVisibility> {
            { "=", ChannelVisibility.Public },
            { "@", ChannelVisibility.Secret },
            { "*", ChannelVisibility.Other  },
        };

        // Numeric 353
        //	( '=' / '*' / '@' ) <channel> ' ' : [ '@' / '+' ] <nick> *( ' ' [ '@' / '+' ] <nick> )
        private void HandleRplNamReply( object sender, MessageEventArgs ev ) {
            if ( !ChannelVisibilityStringMap.ContainsKey( ev.Message.Args[0] ) ) {
                Debug.Print( "NamesHandler.HandleRplNamReply: warning: unknown channel type '{0}'", ev.Message.Args[0] );
            } else {
                ChannelVisibility = ChannelVisibilityStringMap[ev.Message.Args[0]];
            }
            Target = ev.Message.Args[1];

            foreach ( var nickName in ev.Message.Args[2].Split( CommonDelimiters.Space, StringSplitOptions.RemoveEmptyEntries ) ) {
                _nickNames.Add( nickName );
            }
        }

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
