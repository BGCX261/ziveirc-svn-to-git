using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Zive.Irc.Utility;

// ReSharper disable InconsistentNaming

namespace Zive.Irc.Core {

    public class Server {

        //===============
        //   Interface
        //===============

        //
        // Properties
        //

        protected ProtocolHandler _protocolHandler;
        public virtual ProtocolHandler ProtocolHandler {
            get { return _protocolHandler; }
            set {
                if ( null != _protocolHandler ) {
                    UnregisterMessages( );
                }
                _protocolHandler = value;
                if ( null != _protocolHandler ) {
                    RegisterMessages( );
                }
            }
        }

        public virtual SelfUser SelfUser { get; set; }

        public virtual SslEndPoint ServerEndPoint { get; set; }
        public virtual string ServerHostName { get; set; }
        protected string _serverName;
        public virtual string ServerName { get { return _serverName ?? ServerHostName; } set { _serverName = value; } }
        public virtual string ServerPassword { get; set; }

        public virtual Dictionary<string, Channel> Channels { get { return _channels; } }
        public virtual Dictionary<string, User> Users { get { return _users; } }

        public virtual ServerInformation Information { get { return _serverInformation; } }

        public virtual Collection<string> MessageOfTheDay { get { return _motd; } }

        //
        // Events
        //

        // Regular events

        public virtual event EventHandler<MotdCompleteEventArgs>  MotdComplete;
        public virtual event EventHandler<NamesCompleteEventArgs> NamesComplete;
        public virtual event EventHandler<RegisteredEventArgs>    Registered;
        public virtual event EventHandler<WhoCompleteEventArgs>   WhoComplete;
        public virtual event EventHandler<WhoisCompleteEventArgs> WhoisComplete;
        public virtual event EventHandler<MessageEventArgs>       Unknown;

        // Targeted events

        public TargetedEventManager EventManager = new TargetedEventManager( );

        //
        // Constructors
        //

        //
        // TODO
        //
        // User-related:    INVITE KILL QUIT
        // Channel-related: KICK NICK PART
        //

        public Server( ) {
            _verbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>> {
                { "001",        HandleRplWelcome        },
                { "002",        HandleRplYourHost       },
                { "003",        HandleRplCreated        },
                { "004",        HandleRplMyInfo         },
                { "005",        HandleRplISupport       },
                { "311",        HandleRplWhoisUser      },
                { "324",        HandleRplChannelModeIs  },
                { "329",        HandleRplCreationTime   },
                { "332",        HandleRplTopic          },
                { "333",        HandleRplTopicWhoTime   },
                { "352",        HandleRplWhoReply       },
                { "353",        HandleRplNamReply       },
                { "375",        HandleRplMotdStart      },

                { "CAP",        HandleCap               },
                { "ERROR",      HandleError             },
                { "JOIN",       HandleJoin              },
                { "MODE",       HandleMode              },
                { "NOTICE",     HandleNotice            },
                { "PING",       HandlePing              },
                { "PRIVMSG",    HandlePrivMsg           },
                { "QUIT",       HandleQuit              },

                { string.Empty, HandleUnknown           },
            };

            _iSupportHandler = new ISupportHandler( _supportedFeatures );
            _serverInformation = _iSupportHandler.ServerInformation;
        }

        //
        // Methods
        //

        public void SendUserRegistration( ) {
            if ( null == _protocolHandler ) {
                throw new InvalidOperationException( "Server.ProtocolHandler property must be set before using SendUserRegistration method" );
            }

            Debug.Print( "Server.SendUserRegistration" );

            if ( !string.IsNullOrWhiteSpace( ServerPassword ) ) {
                _protocolHandler.SendToServer( "PASS {0}", ServerPassword );
            }

            var capNego = new CapNegotiator {
                Server = this,
            };
            capNego.Complete += CapNegotiatorComplete;

            _protocolHandler.SaveAndClearHandlers( );
            capNego.RegisterMessages( );
            capNego.Start( );
        }

        private void CapNegotiatorComplete( object sender, EventArgs eventArgs ) {
            var capNego = (CapNegotiator) sender;
            _protocolHandler.RestoreHandlers( );
            _protocolHandler.SendToServer( "NICK {0}", SelfUser.NickName );
            _protocolHandler.SendToServer(
                "USER {0} {1} {2} :{3}",
                SelfUser.UserName,
                string.IsNullOrWhiteSpace( SelfUser.RealHostName ) ? "dummy" : SelfUser.RealHostName,
                ServerHostName,
                SelfUser.RealName
            );
        }

        public void SendNick( string newNick ) {
            if ( null == _protocolHandler ) {
                throw new InvalidOperationException( "Server.ProtocolHandler property must be set before using SendNick method" );
            }

            _protocolHandler.SendToServer( "NICK {0}", newNick );
        }

        public void SendQuit( ) {
            SendQuit( null );
        }

        public void SendQuit( string reason ) {
            if ( null == _protocolHandler ) {
                throw new InvalidOperationException( "Server.ProtocolHandler property must be set before using SendQuit method" );
            }

            string text = "QUIT :{0}";
            if ( null == reason ) {
                reason = DefaultQuitMessage;
            } else if ( string.IsNullOrWhiteSpace( reason ) ) {
                text = "QUIT";
                reason = string.Empty;
            }
            _protocolHandler.SendToServer( text, reason );
        }

        public void RegisterChannel( string p, Channel channel ) {
            _channels.ReplaceOrAdd( p, channel );
        }

        public void UnregisterChannel( string p ) {
            _channels.RemoveIfContains( p );
        }

        public Channel LookUpChannel( string name ) {
            return ( _channels.ContainsKey( name ) ) ? _channels[ name ] : null;
        }

        public void RegisterUser( string p, User user ) {
            _users.ReplaceOrAdd( p, user );
        }

        public void UnregisterUser( string p ) {
            _users.RemoveIfContains( p );
        }

        public User LookUpUser( string name ) {
            return ( _users.ContainsKey( name ) ) ? _users[ name ] : null;
        }

        // augh, this is awkward. this is a weird place for IsChannelName, but where else can it go, given what it needs to access?

        public bool IsChannelName( string name ) {
            var channelPrefixCharacters = _supportedFeatures.HasFeature( "CHANTYPES" ) ? _supportedFeatures.GetFeatureValue( "CHANTYPES" ) : "#&";
            return ( name.Length > 0 && channelPrefixCharacters.IndexOf( name[ 0 ] ) > -1 );
        }

        //====================
        //   Implementation
        //====================

        //
        // Fields
        //

        // Constants

        private const string DefaultQuitMessage = "changing universes";

        // Private static read-only

        private static readonly BooleanSwitch DebugDumpEventEnable = new BooleanSwitch( "DebugDumpEventEnable", "Controls whether DebugDumpEvent generates output.", "false" );

        // Private read-only

        private readonly Dictionary<string, EventHandler<MessageEventArgs>> _verbsToRegister;
        private readonly SupportedFeatureCollection _supportedFeatures = new SupportedFeatureCollection( );
        private readonly ServerInformation _serverInformation;
        private readonly ISupportHandler _iSupportHandler;

        // Private

        private Dictionary<string, Channel> _channels = new Dictionary<string, Channel>( );
        private Dictionary<string, User> _users = new Dictionary<string, User>( );
        private Collection<string> _motd;

        private bool _registered;

        //
        // Methods
        //

        private void DebugDumpEvent( MessageEventArgs ev ) {
            if ( DebugDumpEventEnable.Enabled ) {
                Debug.Print( "{0}|{1} => {2}|'{3}'", ev.Message.Verb, ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            }
        }

        private void RegisterMessages( ) {
            foreach ( var item in _verbsToRegister ) {
                _protocolHandler.MessageDiscriminator[ item.Key ] += item.Value;
            }
        }

        private void UnregisterMessages( ) {
            foreach ( var item in _verbsToRegister ) {
                _protocolHandler.MessageDiscriminator[ item.Key ] -= item.Value;
            }
        }

        private void DispatchUserMessage( Message message, string eventKey ) {
            Dictionary<object, EventHandler<MessageEventArgs>> delegates = EventManager[ eventKey ];

            if ( !_users.ContainsKey( message.Target ) ) {
                return;
            }
            var target = _users[ message.Target ];

            if ( !delegates.ContainsKey( target ) ) {
                return;
            }
            var handler = delegates[ target ];

            object origin = this;
            if ( null != message.Origin && _users.ContainsKey( message.Origin.NickName ) ) {
                origin = _users[ message.Origin.NickName ];
            }

            if ( null != handler ) {
                handler( origin, new MessageEventArgs( message ) );
            }
        }

        private void DispatchChannelMessage( Message message, string eventKey ) {
            Dictionary<object, EventHandler<MessageEventArgs>> delegates = EventManager[ eventKey ];

            if ( !_channels.ContainsKey( message.Target ) ) {
                return;
            }
            var target = _channels[ message.Target ];

            if ( !delegates.ContainsKey( target ) ) {
                return;
            }
            var handler = delegates[ target ];

            object origin = this;
            if ( null != message.Origin && _users.ContainsKey( message.Origin.NickName ) ) {
                origin = _users[ message.Origin.NickName ];
            }

            if ( null != handler ) {
                handler( origin, new MessageEventArgs( message ) );
            }
        }

        //
        // Events from children
        //

        // MotdHandler

        private void HandleMotdComplete( object sender, MotdCompleteEventArgs ev ) {
            Debug.Print( "Server.HandleMotdComplete" );
            _motd = ev.Lines;
            _protocolHandler.RestoreHandlers( );
            OnMotdComplete( ev.Lines );
        }

        // NamesHandler

        private void HandleNamesComplete( object sender, NamesCompleteEventArgs ev ) {
            Debug.Print( "Server.HandleNamesComplete" );
            _protocolHandler.RestoreHandlers( );
            OnNamesComplete( ev.NickNames );
        }

        // WhoHandler

        private WhoResponseParser _whoResponseParser;
        private Collection<WhoResponse> _whoResponses;

        private void HandleRawWhoComplete( object sender, EventArgs ev ) {
            Debug.Print( "Server.HandleRawWhoComplete" );
            _protocolHandler.RestoreHandlers( );

            OnWhoComplete( ( (WhoHandler) sender ).Target, _whoResponses );

            _whoResponseParser = null;
            _whoResponses = null;
        }

        private void HandleRawWhoMessage( object sender, MessageEventArgs ev ) {
            if ( null == _whoResponseParser ) {
                _whoResponseParser = new WhoResponseParser {
                    ProtocolHandler = _protocolHandler,
                    Server = this,
                };
            }
            if ( null == _whoResponses ) {
                _whoResponses = new Collection<WhoResponse>( );
            }

            if ( ev.Message.Verb.Equals( "352" ) ) {
                try {
                    _whoResponses.Add( _whoResponseParser.ParseResponse( ev.Message ) );
                }
                catch ( Exception e ) {
                    Debug.Print( "Server.HandleRawWhoMessage: caught exception trying to parse 352 message:\n{0}", e );
                }
            }
        }

        // WhoisHandler

        private void HandleWhoisComplete( object sender, WhoisCompleteEventArgs ev ) {
            Debug.Print( "Server.HandleWhoisComplete: target={0}", ev.Target );
            _protocolHandler.RestoreHandlers( );
            OnWhoisComplete( ev.Target, ev.Responses );
        }

        //
        // Numeric event handlers
        //

        // Numeric 001
        private void HandleRplWelcome( object sender, MessageEventArgs ev ) {
            _registered = true;
            SelfUser.NickName = ev.Message.Target;

            var s = ev.Message.Args[ ev.Message.Args.Count - 1 ];
            var index = s.LastIndexOf( ' ' );
            if ( index > -1 ) {
                SelfUser.NickUserHost = new NickUserHost( s.Substring( index + 1 ) );
            }

            OnRegistered( SelfUser.NickName );
        }

        // Numeric 002
        private void HandleRplYourHost( object sender, MessageEventArgs ev ) {
        }

        // Numeric 003
        private void HandleRplCreated( object sender, MessageEventArgs ev ) {
        }

        // Numeric 004
        private void HandleRplMyInfo( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            _serverName = ev.Message.Args[ 0 ];
            _serverInformation.ServerSoftware = ev.Message.Args[ 1 ];
            _serverInformation.UserModes.All = ev.Message.Args[ 2 ];
        }

        // Numeric 005
        private void HandleRplISupport( object sender, MessageEventArgs ev ) {
            _supportedFeatures.ParseFeatureList( ev.Message.Args );
        }

        // Numeric 311
        private void HandleRplWhoisUser( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            _protocolHandler.SaveAndClearHandlers( );
            var whoisHandler = new WhoisHandler { Server = this, };
            whoisHandler.Complete += HandleWhoisComplete;
            whoisHandler.RegisterMessages( );
            whoisHandler.HandleRplWhoisUser( sender, ev );
        }

        // Numeric 324
        private void HandleRplChannelModeIs( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnRplChannelModeIs( ev.Message );
        }

        // Numeric 329
        private void HandleRplCreationTime( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnRplCreationTime( ev.Message );
        }

        // Numeric 332
        private void HandleRplTopic( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnRplTopic( ev.Message );
        }

        // Numeric 333
        private void HandleRplTopicWhoTime( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnRplTopicWhoTime( ev.Message );
        }

        // Numeric 352
        private void HandleRplWhoReply( object sender, MessageEventArgs ev ) {
            _protocolHandler.SaveAndClearHandlers( );
            var whoHandler = new WhoHandler { Server = this, };
            whoHandler.Complete += HandleRawWhoComplete;
            whoHandler.Response += HandleRawWhoMessage;
            whoHandler.RegisterMessages( );
            whoHandler.HandleRplWhoReply( sender, ev );
        }

        // Numeric 353
        private void HandleRplNamReply( object sender, MessageEventArgs ev ) {
            _protocolHandler.SaveAndClearHandlers( );
            var namesHandler = new NamesHandler { Server = this, };
            namesHandler.Complete += HandleNamesComplete;
            namesHandler.RegisterMessages( );
            namesHandler.HandleRplNamReply( sender, ev );
        }

        // Numeric 375
        private void HandleRplMotdStart( object sender, MessageEventArgs ev ) {
            _protocolHandler.SaveAndClearHandlers( );
            var motdHandler = new MotdHandler { Server = this, };
            motdHandler.Complete += HandleMotdComplete;
            motdHandler.RegisterMessages( );
            motdHandler.HandleRplMotdStart( sender, ev );
        }

        //
        // Verb event handlers
        //

        private void HandleCap( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnCap( ev.Message );
        }

        private void HandleError( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            ev.Message.Target = SelfUser.NickName;
            OnError( ev.Message );
        }

        private void HandleJoin( object sender, MessageEventArgs ev ) {
            ev.Message.Target = ev.Message.Args[ 0 ];
            var channel = new Channel {
                Name = ev.Message.Target,
                ProtocolHandler = _protocolHandler,
                SelfUser = SelfUser,
                Server = this
            };
            DebugDumpEvent( ev );
            OnJoin( ev.Message );
        }

        private void HandleMode( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            if ( IsChannelName( ev.Message.Target ) ) {
                OnChannelMode( ev.Message );
            } else {
                OnUserMode( ev.Message );
            }
        }

        private void HandleNotice( object sender, MessageEventArgs ev ) {
            if ( null == ev.Message.Target ) {
                ev.Message.Target = (string) SelfUser.NickUserHost;
            } else {
                if ( !_registered && !IsChannelName( ev.Message.Target ) && !_users.ContainsKey( ev.Message.Target ) ) {
                    ev.Message.Target = SelfUser.NickName;
                }
            }
            DebugDumpEvent( ev );
            OnNotice( ev.Message );
        }

        private void HandlePing( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            _protocolHandler.SendToServer( "PONG :{0}", string.Join( " ", ev.Message.Args ) );
        }

        private void HandlePrivMsg( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            OnPrivMsg( ev.Message );
        }

        private void HandleQuit( object sender, MessageEventArgs ev ) {
            Debug.Print( "Server.HandleQuit: Origin={0} SelfUser.NickUserHost={1}", ev.Message.Origin, SelfUser.NickUserHost );
            DebugDumpEvent( ev );
            OnQuit( ev.Message );
        }

        private void HandleUnknown( object sender, MessageEventArgs ev ) {
            Debug.Print( "Server.HandleUnknown: {0}", ev.Message );
            DebugDumpEvent( ev );
            OnUnknown( ev.Message );
        }

        //
        // Regular event dispatchers
        //

        private void OnMotdComplete( Collection<string> lines ) {
            var handler = MotdComplete;
            if ( null != handler ) {
                handler( this, new MotdCompleteEventArgs( lines ) );
            }
        }

        private void OnNamesComplete( Collection<string> nickNames ) {
            var handler = NamesComplete;
            if ( null != handler ) {
                handler( this, new NamesCompleteEventArgs( nickNames ) );
            }
        }

        private void OnRegistered( string nickName ) {
            var handler = Registered;
            if ( null != handler ) {
                handler( this, new RegisteredEventArgs( nickName ) );
            }
        }

        private void OnWhoComplete( object target, Collection<WhoResponse> responses ) {
            var handler = WhoComplete;
            if ( null != handler ) {
                handler( this, new WhoCompleteEventArgs( target, responses ) );
            }
        }

        private void OnWhoisComplete( NickUserHost target, Collection<Message> responses ) {
            var handler = WhoisComplete;
            if ( null != handler ) {
                handler( this, new WhoisCompleteEventArgs( target, responses ) );
            }
        }

        //
        // Channel-specific event dispatchers
        //

        private void OnChannelMode( Message message ) {
            // TODO is Target correct?
            DispatchChannelMessage( message, "MODE:channel" );
        }

        private void OnJoin( Message message ) {
            DispatchChannelMessage( message, "JOIN" );
        }

        private void OnRplChannelModeIs( Message message ) {
            message.Target = message.Args[ 0 ];
            DispatchChannelMessage( message, "RplChannelModeIs" );
        }

        private void OnRplCreationTime( Message message ) {
            message.Target = message.Args[ 0 ];
            DispatchChannelMessage( message, "RplCreationTime" );
        }

        private void OnRplTopic( Message message ) {
            message.Target = message.Args[ 0 ];
            DispatchChannelMessage( message, "RplTopic" );
        }

        private void OnRplTopicWhoTime( Message message ) {
            message.Target = message.Args[ 0 ];
            DispatchChannelMessage( message, "RplTopicWhoTime" );
        }

        //
        // User-specific event dispatchers
        //

        private void OnCap( Message message ) {
            DispatchUserMessage( message, "CAP" );
        }

        private void OnError( Message message ) {
            DispatchUserMessage( message, "ERROR" );
        }

        private void OnNotice( Message message ) {
            DispatchUserMessage( message, "NOTICE" );
        }

        private void OnPrivMsg( Message message ) {
            DispatchUserMessage( message, "PRIVMSG" );
        }

        private void OnQuit( Message message ) {
            DispatchUserMessage( message, "QUIT" );
        }

        private void OnUserMode( Message message ) {
            DispatchUserMessage( message, "MODE:user" );
        }

        //
        // Unknown event dispatcher
        //

        private void OnUnknown( Message message ) {
            var handler = Unknown;
            if ( null != handler ) {
                handler( this, new MessageEventArgs( message ) );
            }
        }

        //
        // Implementation of Object
        //

        public override string ToString( ) {
            return string.Format( "{0} ({1})", ServerName, ServerEndPoint );
        }

    }

}
