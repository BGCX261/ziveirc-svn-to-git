using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Zive.Irc.Core.Annotations;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class Channel: INotifyPropertyChanged, ITarget {

        //===============
        //   Interface
        //===============

        //
        // Properties
        //

        public virtual ProtocolHandler ProtocolHandler { get; set; }
        public virtual SelfUser SelfUser { get; set; }
        public virtual Server Server {
            get { return _server; }
            set {
                if ( null != _server ) {
                    UnregisterServerHandlers( );
                }
                _server = value;
                if ( null != _server ) {
                    RegisterServerHandlers( );
                }
                OnPropertyChanged( "Server" );
            }
        }

        public virtual string Name { get; set; }

        public virtual string Topic { get; protected set; }
        public virtual NickUserHost TopicSetBy { get; protected set; }
        public virtual DateTime TopicSetAt { get; protected set; }

        public virtual Collection<User> Users { get; protected set; }
        public virtual Dictionary<User, string> UserFlags { get; protected set; }

        //
        // Events
        //

        //
        // Constructors
        //

        public Channel( ) {
            _targetedEvents = new List<TargetedEvent> {
                new TargetedEvent { EventKey = "JOIN",             Handler = HandleJoin             },
                new TargetedEvent { EventKey = "RplChannelModeIs", Handler = HandleRplChannelModeIs },
                new TargetedEvent { EventKey = "RplCreationTime",  Handler = HandleRplCreationTime  },
                new TargetedEvent { EventKey = "RplTopic",         Handler = HandleRplTopic         },
                new TargetedEvent { EventKey = "RplTopicWhoTime",  Handler = HandleRplTopicWhoTime  },
            };
        }

        //
        // Methods
        //

        public override string ToString( ) {
            return Name;
        }

        //====================
        //   Implementation
        //====================

        //
        // Fields
        //

        // Protected

        protected Server _server;
        protected List<TargetedEvent> _targetedEvents;

        //
        // Methods
        //

        protected virtual void RegisterServerHandlers( ) {
            if ( null != Name ) {
                _server.RegisterChannel( Name, this );
            }

            _server.NamesComplete += HandleNamesComplete;
            _server.WhoComplete += HandleWhoComplete;

            _server.EventManager.BulkSubscribe( _targetedEvents );
        }

        protected virtual void UnregisterServerHandlers( ) {
            _server.EventManager.BulkUnsubscribe( _targetedEvents );

            _server.NamesComplete -= HandleNamesComplete;
            _server.WhoComplete -= HandleWhoComplete;

            if ( null != Name ) {
                _server.UnregisterChannel( Name );
            }
        }

        //
        // Event handlers
        //

        // Handler object events

        private void HandleNamesComplete( object sender, NamesCompleteEventArgs ev ) {
            Debug.Print( "Channel.HandleNamesComplete: typeof(sender)={0} sender={1} channel={2} nicks={3}", sender.GetType( ), sender, Name, string.Join( ",", ev.NickNames ) );
            if ( null == Users ) {
                Users = new Collection<User>( );
            } else {
                Users.Clear( );
            }
            if ( null == UserFlags ) {
                UserFlags = new Dictionary<User, string>( );
            } else {
                UserFlags.Clear( );
            }

            var flagList = string.IsNullOrEmpty( Server.Information.ChannelModes.StatusSigils ) ? "@+" : Server.Information.ChannelModes.StatusSigils;
            foreach ( var item in ev.NickNames ) {
                var flags = string.Empty;
                for ( var i = 0; i < item.Length; i++ ) {
                    if ( flagList.IndexOf( item[ i ] ) > -1 ) {
                        flags += item[ i ];
                    } else {
                        var channelMember = new NickUserHost( item.Substring( i ) );
                        var user = Server.LookUpUser( channelMember.NickName ) ?? new User {
                            NickUserHost = channelMember,
                            ProtocolHandler = ProtocolHandler,
                            Server = Server,
                        };
                        Users.Add( user );
                        UserFlags.Add( user, flags );
                        break;
                    }
                }
            }
        }

        private void HandleWhoComplete( object sender, WhoCompleteEventArgs ev ) {
            Debug.Print( "Channel.HandleWhoComplete: typeof(sender)={0} sender={1} target={2}", sender.GetType( ), sender, ev.Target );
        }

        // Numeric events

        // Numeric 324
        private void HandleRplChannelModeIs( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplChannelModeIs: typeof(sender)={0} sender={1} args='{2}'", sender.GetType( ), sender, string.Join( "','", ev.Message.Args ) );
        }

        // Numeric 329
        private void HandleRplCreationTime( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplChannelModeIs: typeof(sender)={0} sender={1} args='{2}'", sender.GetType( ), sender, string.Join( "','", ev.Message.Args ) );
        }

        // Numeric 332
        private void HandleRplTopic( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplTopic: typeof(sender)={0} sender={1} args='{2}'", sender.GetType( ), sender, string.Join( "','", ev.Message.Args ) );
            Topic = ev.Message.Args[ 0 ];
            OnPropertyChanged( "Topic" );
        }

        // Numeric 333
        private void HandleRplTopicWhoTime( object sender, MessageEventArgs ev ) {
            Debug.Print( "Channel.HandleRplTopicWhoTime: typeof(sender)={0} sender={1} args='{2}'", sender.GetType( ), sender, string.Join( "','", ev.Message.Args ) );

            try {
                TopicSetBy = new NickUserHost( ev.Message.Args[ 1 ] );
                OnPropertyChanged( "TopicSetBy" );
                Debug.Print( "Channel.HandleRplTopicWhoTime: topic set by '{0}'", TopicSetBy );
            }
            catch ( Exception e ) {
                Debug.Print( "Channel.HandleRplTopicWhoTime: while setting TopicSetBy: caught exception\n{0}", e );
            }

            try {
                TopicSetAt = UnixTimestampConverter.FromStringSeconds( ev.Message.Args[ 2 ] );
                OnPropertyChanged( "TopicSetAt" );
                Debug.Print( "Channel.HandleRplTopicWhoTime: topic set at {0}", TopicSetAt.ToString( "s" ) );
            }
            catch ( Exception e ) {
                Debug.Print( "Channel.HandleRplTopicWhoTime: while setting TopicSetAt: caught exception\n{0}", e );
            }
        }

        // Verb events

        private void HandleJoin( object sender, MessageEventArgs ev ) {
            if ( null != Name && !Name.Equals( ev.Message.Target, StringComparison.OrdinalIgnoreCase ) ) {
                Debug.Print( "Channel.HandleJoin: changing name from '{0}' to '{1}'", Name, ev.Message.Target );
                if ( null != Name ) {
                    _server.UnregisterChannel( Name );
                    Name = null;
                }
                Name = ev.Message.Target;
                if ( null != Name ) {
                    _server.RegisterChannel( Name, this );
                }
            }

            ProtocolHandler.SendToServer( "WHO {0}", Name );
        }

        //
        // INotifyPropertyChanged
        //

        // Events

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( string propertyName ) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

    }

}
