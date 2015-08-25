using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Zive.Irc.Core.Annotations;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class Channel: INotifyPropertyChanged, ITargetable {

        //===============
        //   Interface
        //===============

        //
        // Properties
        //

        public virtual string Name {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged( );
            }
        }

        public virtual Server Server {
            get { return _server; }
            set {
                if ( null != _protocolHandler ) {
                    _protocolHandler.Unsubscribe( _messageFilters );
                }
                _server = value;
                if ( null != _server ) {
                    _protocolHandler = _server.ProtocolHandler;
                    _protocolHandler.Subscribe( _messageFilters );
                } else {
                    _protocolHandler = null;
                }
                OnPropertyChanged( );
            }
        }

        public virtual SelfUser SelfUser {
            get { return _selfUser; }
            set {
                _selfUser = value;
                OnPropertyChanged( );
            }
        }

        public virtual string Topic {
            get { return _topic; }
            set {
                _topic = value;
                OnPropertyChanged( );
            }
        }

        public virtual NickUserHost TopicSetBy {
            get { return _topicSetBy; }
            set {
                _topicSetBy = value;
                OnPropertyChanged( );
            }
        }

        public virtual DateTime TopicSetAt {
            get { return _topicSetAt; }
            set {
                _topicSetAt = value;
                OnPropertyChanged( );
            }
        }

        public virtual Collection<User> Users {
            get { return _users; }
            set {
                _users = value;
                OnPropertyChanged( );
            }
        }

        public virtual Dictionary<User, string> UserFlags {
            get { return _userFlags; }
            set {
                _userFlags = value;
                OnPropertyChanged( );
            }
        }

        //
        // Events
        //

        //
        // Constructors
        //

        public Channel( ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "JOIN" ), HandleJoin             ),
                new FilterAndHandler( ( m ) => ( m.Verb == "324"  ), HandleRplChannelModeIs ),
                new FilterAndHandler( ( m ) => ( m.Verb == "329"  ), HandleRplCreationTime  ),
                new FilterAndHandler( ( m ) => ( m.Verb == "332"  ), HandleRplTopic         ),
                new FilterAndHandler( ( m ) => ( m.Verb == "333"  ), HandleRplTopicWhoTime  ),
            };
        }

        //
        // Methods
        //

        public override string ToString( ) {
            return _name;
        }

        //====================
        //   Implementation
        //====================

        //
        // Fields
        //

        // Property backing fields

        protected Server _server;
        protected ProtocolHandler _protocolHandler;
        protected SelfUser _selfUser;
        protected string _name;
        protected string _topic;
        protected NickUserHost _topicSetBy;
        protected DateTime _topicSetAt;
        protected Collection<User> _users;
        protected Dictionary<User, string> _userFlags;

        // Other fields

        protected readonly List<FilterAndHandler> _messageFilters;

        //
        // Methods
        //

        protected virtual void SubscribeMessageFilters( ) {
            _server.NamesComplete += HandleNamesComplete;
            _server.WhoComplete += HandleWhoComplete;

            _server.ProtocolHandler.Subscribe( _messageFilters );
        }

        protected virtual void UnsubscribeMessageFilters( ) {
            _server.ProtocolHandler.Unsubscribe( _messageFilters );

            _server.NamesComplete -= HandleNamesComplete;
            _server.WhoComplete -= HandleWhoComplete;
        }

        //
        // Event handlers
        //

        // Handler object events

        private void HandleNamesComplete( object sender, NamesCompleteEventArgs ev ) {
            Debug.Print( "Channel.HandleNamesComplete: channel={0} nicks={1}", _name, string.Join( ",", ev.NickNames ) );
            Users = new Collection<User>( );
            UserFlags = new Dictionary<User, string>( );

            var flagList = string.IsNullOrEmpty( _server.Information.ChannelModes.StatusSigils ) ? "@+" : _server.Information.ChannelModes.StatusSigils;
            foreach ( var item in ev.NickNames ) {
                var flags = string.Empty;
                for ( var i = 0; i < item.Length; i++ ) {
                    if ( flagList.IndexOf( item[ i ] ) > -1 ) {
                        flags += item[ i ];
                    } else {
                        var channelMember = new NickUserHost( item.Substring( i ) );
                        var user = _server.LookUpOrRegisterUser( channelMember );
                        _users.Add( user );
                        _userFlags.Add( user, flags );
                        break;
                    }
                }
            }
        }

        private void HandleWhoComplete( object sender, WhoCompleteEventArgs ev ) {
            Debug.Print( "Channel.HandleWhoComplete: target={0}", ev.Target );
            // TODO
        }

        // Numeric events

        // Numeric 324
        private void HandleRplChannelModeIs( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplChannelModeIs: args='{0}'", string.Join( "','", ev.Message.Args ) );
            // TODO
        }

        // Numeric 329
        private void HandleRplCreationTime( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplCreationTime: args='{0}'", string.Join( "','", ev.Message.Args ) );
            // TODO
        }

        // Numeric 332
        private void HandleRplTopic( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplTopic: args='{0}'", string.Join( "','", ev.Message.Args ) );
            Topic = ev.Message.Args[0];
            // TODO
        }

        // Numeric 333
        private void HandleRplTopicWhoTime( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplTopicWhoTime: args='{0}'", string.Join( "','", ev.Message.Args ) );

            try {
                TopicSetBy = new NickUserHost( ev.Message.Args[1] );
                Debug.Print( "Channel.HandleRplTopicWhoTime: topic set by '{0}'", _topicSetBy );
            }
            catch ( Exception e ) {
                Debug.Print( "Channel.HandleRplTopicWhoTime: while setting TopicSetBy: caught exception\n{0}", e );
            }

            try {
                TopicSetAt = UnixTimestampConverter.FromStringSeconds( ev.Message.Args[2] );
                Debug.Print( "Channel.HandleRplTopicWhoTime: topic set at {0}", _topicSetAt.ToString( "s" ) );
            }
            catch ( Exception e ) {
                Debug.Print( "Channel.HandleRplTopicWhoTime: while setting TopicSetAt: caught exception\n{0}", e );
            }
        }

        // Verb events

        private void HandleJoin( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleJoin: ev.Message={0}", ev.Message );
            if ( !string.IsNullOrEmpty( _name ) && !_name.Equals( ev.Message.Target, StringComparison.Ordinal ) ) {
                Debug.Print( "Channel.HandleJoin: changing name from '{0}' to '{1}'", _name, ev.Message.Target );
                _server.UnregisterChannel( _name );
                Name = ev.Message.Target;
                _server.RegisterChannel( _name, this );
            }

            _protocolHandler.SendToServer( "WHO {0}", _name );
        }

        //
        // INotifyPropertyChanged
        //

        // Events

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" ) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

    }

}
