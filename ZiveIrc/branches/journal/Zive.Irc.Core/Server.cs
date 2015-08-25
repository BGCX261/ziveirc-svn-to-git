using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class Server {

        //===============
        //   Interface
        //===============

        //
        // Properties
        //

        public virtual ProtocolHandler ProtocolHandler {
            get { return _protocolHandler; }
            set {
                if ( null != _protocolHandler ) {
                    _protocolHandler.Unsubscribe( _messageFilters );
                }
                _protocolHandler = value;
                if ( null != _protocolHandler ) {
                    _protocolHandler.Subscribe( _messageFilters );
                }
            }
        }

        public virtual SelfUser SelfUser {
            get { return _selfUser; }
            set { _selfUser = value; }
        }

        public virtual SslEndPoint ServerEndPoint {
            get { return _serverEndPoint; }
            set { _serverEndPoint = value; }
        }

        public virtual string ServerHostName {
            get { return _serverHostName; }
            set { _serverHostName = value; }
        }

        public virtual string ServerName {
            get { return _serverName ?? _serverHostName; }
            set { _serverName = value; }
        }

        public virtual string ServerPassword {
            get { return _serverPassword; }
            set { _serverPassword = value; }
        }

        public virtual Dictionary<string, Channel> Channels {
            get { return _channels; }
        }

        public virtual Dictionary<string, User> Users {
            get { return _users; }
        }

        public virtual ServerInformation Information {
            get { return _serverInformation; }
        }

        public virtual IList<string> MessageOfTheDay {
            get { return _messageOfTheDay; }
        }

        public virtual bool IsRegistered {
            get { return _isRegistered; }
        }

        //
        // Events
        //

        public virtual event EventHandler<MotdCompleteEventArgs>  MotdComplete;
        public virtual event EventHandler<NamesCompleteEventArgs> NamesComplete;
        public virtual event EventHandler<RegisteredEventArgs>    Registered;
        public virtual event EventHandler<ServerNoticeEventArgs>  ServerNotice;
        public virtual event EventHandler<WhoCompleteEventArgs>   WhoComplete;
        public virtual event EventHandler<WhoisCompleteEventArgs> WhoisComplete;

        //
        // TODO
        //
        // User-related:    INVITE KILL QUIT
        // Channel-related: KICK NICK PART
        //

        //
        // Constructors
        //

        public Server( ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "001"   ), HandleRplWelcome   ),
                new FilterAndHandler( ( m ) => ( m.Verb == "004"   ), HandleRplMyInfo    ),
                new FilterAndHandler( ( m ) => ( m.Verb == "005"   ), HandleRplISupport  ),
                new FilterAndHandler( ( m ) => ( m.Verb == "311"   ), HandleRplWhoisUser ),
                new FilterAndHandler( ( m ) => ( m.Verb == "352"   ), HandleRplWhoReply  ),
                new FilterAndHandler( ( m ) => ( m.Verb == "353"   ), HandleRplNamReply  ),
                new FilterAndHandler( ( m ) => ( m.Verb == "375"   ), HandleRplMotdStart ),
                new FilterAndHandler( ( m ) => ( m.Verb == "422"   ), HandleErrNoMotd    ),
                new FilterAndHandler( ( m ) => ( m.Verb == "ERROR" ), HandleError        ),
                new FilterAndHandler( ( m ) => ( m.Verb == "JOIN"  ), HandleJoin         ),
                new FilterAndHandler( ( m ) => ( m.Verb == "PING"  ), HandlePing         ),

                new FilterAndHandler( ( m ) => ( m.Verb == "NOTICE" && m.Origin.IsServerName ), HandleServerNotice ),
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

            _protocolHandler.StartCapture( "Server.SendUserRegistration running CapNegotiator" );
            var capNego = new CapNegotiator( this, CapNegotiatorComplete );
            capNego.Start( );
        }

        private void CapNegotiatorComplete( object sender, EventArgs eventArgs ) {
            var capNego = (CapNegotiator) sender;
            _protocolHandler.EndCapture( "Server.SendUserRegistration running CapNegotiator" );
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
            _protocolHandler.SendToServer( text, reason ?? DefaultQuitMessage );
        }

        public void RegisterChannel( string p, Channel channel ) {
            _channels.ReplaceOrAdd( p, channel );
        }

        public void UnregisterChannel( string p ) {
            _channels.RemoveIfContains( p );
        }

        public Channel LookUpChannel( string name ) {
            return ( _channels.ContainsKey( name ) ) ? _channels[name] : null;
        }

        public void RegisterUser( string p, User user ) {
            _users.ReplaceOrAdd( p, user );
        }

        public void UnregisterUser( string p ) {
            _users.RemoveIfContains( p );
        }

        public User LookUpUser( string name ) {
            return ( _users.ContainsKey( name ) ) ? _users[name] : null;
        }

        public User LookUpOrRegisterUser( NickUserHost userhost ) {
            var nick = userhost.NickName;
            var user = LookUpUser( nick );
            if ( null == user ) {
                user = new User {
                    NickUserHost = userhost,
                    Server = this
                };
                RegisterUser( nick, user );
            }
            return user;
        }

        public ITargetable LookUpTarget( string name ) {
            return IsChannelName( name ) ? (ITargetable) LookUpChannel( name ) : (ITargetable) LookUpUser( name );
        }

        // augh, this is awkward. this is a weird place for IsChannelName, but where else can it go, given what it needs to access?

        public bool IsChannelName( string name ) {
            var channelPrefixCharacters = _supportedFeatures.HasFeature( "CHANTYPES" ) ? _supportedFeatures.GetFeatureValue( "CHANTYPES" ) : "#&";
            return ( name.Length > 0 && channelPrefixCharacters.IndexOf( name[0] ) > -1 );
        }

        //====================
        //   Implementation
        //====================

        //
        // Fields
        //

        // Constants

        private const string DefaultQuitMessage = "changing universes";

        // Property backing fields

        protected ProtocolHandler _protocolHandler;
        protected SelfUser _selfUser;
        protected SslEndPoint _serverEndPoint;
        protected string _serverHostName;
        protected string _serverName;
        protected string _serverPassword;
        protected Dictionary<string, Channel> _channels = new Dictionary<string, Channel>( );
        protected Dictionary<string, User> _users = new Dictionary<string, User>( );
        private readonly ServerInformation _serverInformation;
        protected IList<string> _messageOfTheDay;

        // Private static read-only

        private static readonly BooleanSwitch DebugDumpEventEnable = new BooleanSwitch( "DebugDumpEventEnable", "Controls whether DebugDumpEvent generates output.", "false" );

        // Private read-only

        private readonly List<FilterAndHandler> _messageFilters;
        private readonly SupportedFeatureCollection _supportedFeatures = new SupportedFeatureCollection( );
        private readonly ISupportHandler _iSupportHandler;

        // Private

        private bool _isRegistered;
        private WhoResponseParser _whoResponseParser;
        private Collection<WhoResponse> _whoResponses;

        //
        // Methods
        //

        private void DebugDumpEvent( MessageEventArgs ev ) {
            if ( DebugDumpEventEnable.Enabled ) {
                Debug.Print( "{0}|{1} => {2}|'{3}'", ev.Message.Verb, ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            }
        }

        //
        // Events from children
        //

        // MotdHandler

        private void HandleMotdComplete( object sender, MotdCompleteEventArgs ev ) {
            Debug.Print( "Server.HandleMotdComplete" );
            _messageOfTheDay = ev.Lines;
            _protocolHandler.EndCapture( "Server.HandleRplMotdStart/HandleErrNoMotd running MotdHandler" );
            OnMotdComplete( ev.Lines );
        }

        // NamesHandler

        private void HandleNamesComplete( object sender, NamesCompleteEventArgs ev ) {
            Debug.Print( "Server.HandleNamesComplete" );
            _protocolHandler.EndCapture( "Server.HandleRplNamReply running NamesHandler" );
            OnNamesComplete( ev.NickNames );
        }

        // WhoHandler

        private void HandleRawWhoComplete( object sender, EventArgs ev ) {
            Debug.Print( "Server.HandleRawWhoComplete" );
            _protocolHandler.EndCapture( "Server.HandleRplWhoReply running WhoHandler" );

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
            if ( null == ev.Target ) {
                Debug.Print( "Server.HandleWhoisComplete: an error occurred during processing" );
                return;
            }

            Debug.Print( "Server.HandleWhoisComplete: target={0}", ev.Target );
            _protocolHandler.EndCapture( "Server.HandleRplWhoisUser running WhoisHandler" );
            OnWhoisComplete( ev.Target, ev.Responses );
        }

        //
        // Numeric event handlers
        //

        // Numeric 001
        private void HandleRplWelcome( object sender, MessageEventArgs ev ) {
            _isRegistered = true;
            SelfUser.NickName = ev.Message.Target;

            var s = ev.Message.Args[ev.Message.Args.Count - 1];
            var index = s.LastIndexOf( ' ' );
            if ( index > -1 ) {
                SelfUser.NickUserHost = new NickUserHost( s.Substring( index + 1 ) );
            }

            OnRegistered( SelfUser.NickName );
        }

        // Numeric 004
        private void HandleRplMyInfo( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            _serverName = ev.Message.Args[0];
            _serverInformation.ServerSoftware = ev.Message.Args[1];
            _serverInformation.UserModes.All = ev.Message.Args[2];
        }

        // Numeric 005
        private void HandleRplISupport( object sender, MessageEventArgs ev ) {
            _supportedFeatures.ParseFeatureList( ev.Message.Args );
        }

        // Numeric 311
        private void HandleRplWhoisUser( object sender, MessageEventArgs ev ) {
            _protocolHandler.StartCapture( "Server.HandleRplWhoisUser running WhoisHandler" );
            var whoisHandler = new WhoisHandler( this, HandleWhoisComplete, ev );
        }

        // Numeric 352
        private void HandleRplWhoReply( object sender, MessageEventArgs ev ) {
            _protocolHandler.StartCapture( "Server.HandleRplWhoReply running WhoHandler" );
            var whoHandler = new WhoHandler( this, HandleRawWhoComplete, HandleRawWhoMessage, ev );
        }

        // Numeric 353
        private void HandleRplNamReply( object sender, MessageEventArgs ev ) {
            _protocolHandler.StartCapture( "Server.HandleRplNamReply running NamesHandler" );
            var namesHandler = new NamesHandler( this, HandleNamesComplete, ev );
        }

        // Numeric 375
        private void HandleRplMotdStart( object sender, MessageEventArgs ev ) {
            _protocolHandler.StartCapture( "Server.HandleRplMotdStart/HandleErrNoMotd running MotdHandler" );
            var motdHandler = new MotdHandler( this, ev, HandleMotdComplete );
        }

        // Numeric 422
        private void HandleErrNoMotd( object sender, MessageEventArgs ev ) {
            _protocolHandler.StartCapture( "Server.HandleRplMotdStart/HandleErrNoMotd running MotdHandler" );
            var motdHandler = new MotdHandler( this, ev, HandleMotdComplete );
        }

        //
        // Verb event handlers
        //

        private void HandleError( object sender, MessageEventArgs ev ) {
            DebugDumpEvent( ev );
            // TODO
        }

        private void HandleJoin( object sender, MessageEventArgs ev ) {
            var target = ev.Message.Args[0];
            var channel = LookUpChannel( target );
            if ( null == channel ) {
                channel = new Channel {
                    Name     = target,
                    Server   = this,
                    SelfUser = SelfUser,
                };
                _channels.Add( channel.Name, channel );
            }
        }

        private void HandleServerNotice( object sender, MessageEventArgs ev ) {
            Debug.Print( "Server.HandleServerNotice: origin='{0}' [target='{1}'] text='{2}'", ev.Message.Origin, ev.Message.Target, ev.Message.Args[0] );
            OnServerNotice( ev.Message.Origin, ev.Message.Args[0] );
        }

        private void HandlePing( object sender, MessageEventArgs ev ) {
            _protocolHandler.SendToServer( "PONG :{0}", string.Join( " ", ev.Message.Args ) );
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

        private void OnServerNotice( NickUserHost origin, string text ) {
            var handler = ServerNotice;
            if ( null != handler ) {
                handler( this, new ServerNoticeEventArgs( origin, text ) );
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
        // Implementation of Object
        //

        public override string ToString( ) {
            return string.Format( "{0} ({1})", ServerName, ServerEndPoint );
        }

    }

}
