using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    internal class BooleanFeatureAttribute: Attribute {

        public string Key { get; set; }

        public BooleanFeatureAttribute( string key ) {
            Key = key;
        }

    }

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    internal class StringFeatureAttribute: Attribute {

        public string Key { get; set; }

        public StringFeatureAttribute( string key ) {
            Key = key;
        }

    }

    // ReSharper disable InconsistentNaming

    public class ISupportHandler {

        //
        // Interface
        //

        // Fields

        // Properties

        public ServerInformation ServerInformation { get; set; }

        // Events

        // Constructors

        public ISupportHandler( ) {
            ServerInformation = new ServerInformation( );

            _booleanFeatureKeyParsers = new Dictionary<string, Action>( );
            _stringFeatureKeyParsers = new Dictionary<string, Action<string>>( );

            foreach ( var method in GetType( ).GetMethods( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod ) ) {
                var bfaType = typeof( BooleanFeatureAttribute );
                foreach ( var attr in method.GetCustomAttributes( bfaType, true ) ) {
                    var booleanFeature = (BooleanFeatureAttribute) attr;
                    var m = method;
                    _booleanFeatureKeyParsers.Add( booleanFeature.Key, ( ) => m.Invoke( this, null ) );
                }

                var sfaType = typeof( StringFeatureAttribute );
                foreach ( var attr in method.GetCustomAttributes( sfaType, true ) ) {
                    var stringFeature = (StringFeatureAttribute) attr;
                    var m = method;
                    _stringFeatureKeyParsers.Add( stringFeature.Key, _ => m.Invoke( this, new object[ ] { _ } ) );
                }
            }
        }

        public ISupportHandler( SupportedFeatureCollection supportedFeatures ): this( ) {
            _supportedFeatures = supportedFeatures;
            _supportedFeatures.BooleanFeatureKeys.CollectionChanged += BooleanFeatureKeysCollectionChanged;
            _supportedFeatures.StringFeatureKeys.CollectionChanged += StringFeatureKeysCollectionChanged;
        }

        // Methods

        //
        // Implementation
        //

        // Fields

        private static readonly Regex PrefixParserRegex = new Regex( @"^\(([^)]+)\)(.*)$" );
        private static readonly Dictionary<char, ListSearchExtensionTypes> EListMap = new Dictionary<char, ListSearchExtensionTypes> {
            { 'M', ListSearchExtensionTypes.Mask },
            { 'N', ListSearchExtensionTypes.NotMask },
            { 'U', ListSearchExtensionTypes.UserCount },
            { 'C', ListSearchExtensionTypes.CreationTime },
            { 'T', ListSearchExtensionTypes.Topic },
        };

        private readonly SupportedFeatureCollection _supportedFeatures;
        private readonly Dictionary<string, Action> _booleanFeatureKeyParsers = new Dictionary<string, Action>( );
        private readonly Dictionary<string, Action<string>> _stringFeatureKeyParsers = new Dictionary<string, Action<string>>( );

        // Properties

        // Events

        // Constructors

        // Methods

        // -- Feature keys collection changed event handlers

        private void BooleanFeatureKeysCollectionChanged( object sender, NotifyCollectionChangedEventArgs ev ) {
            if ( NotifyCollectionChangedAction.Add == ev.Action ) {
                foreach ( string key in ev.NewItems ) {
                    if ( _booleanFeatureKeyParsers.ContainsKey( key ) ) {
                        _booleanFeatureKeyParsers[ key ]( );
                    } else {
                        Debug.Print( "ISupportHandler.BooleanFeatureKeysCollectionChanged: Unhandled key: '{0}'", key );
                    }
                }
            } else {
                Debug.Print( "ISupportHandler.BooleanFeatureKeysCollectionChanged: unsupported Action value {0} received", ev.Action );
            }
        }

        private void StringFeatureKeysCollectionChanged( object sender, NotifyCollectionChangedEventArgs ev ) {
            if ( NotifyCollectionChangedAction.Add == ev.Action ) {
                foreach ( string key in ev.NewItems ) {
                    var value = _supportedFeatures.StringFeatures[ key ];
                    if ( _stringFeatureKeyParsers.ContainsKey( key ) ) {
                        _stringFeatureKeyParsers[ key ]( value );
                    } else {
                        Debug.Print( "ISupportHandler.StringFeatureKeysCollectionChanged: Unhandled key/value pair: '{0}' => '{1}'", key, value );
                    }
                }
            } else {
                Debug.Print( "ISupportHandler.StringFeatureKeysCollectionChanged: unsupported Action value {0} received", ev.Action );
            }
        }

        // -- Boolean features

        [BooleanFeature( "ACCEPT" )]
        protected virtual void _handleAccept( ) {
            ServerInformation.Feature.CallerId = true;
        }

        [BooleanFeature( "CALLERID" )]
        protected virtual void _handleCallerId( ) {
            ServerInformation.Feature.CallerId = true;
        }

        [BooleanFeature( "CNOTICE" )]
        protected virtual void _handleCNotice( ) {
            ServerInformation.Feature.CNotice = true;
        }

        [BooleanFeature( "CPRIVMSG" )]
        protected virtual void _handleCPrivMsg( ) {
            ServerInformation.Feature.CPrivMsg = true;
        }

        [BooleanFeature( "DEAF" )]
        protected virtual void _handleDeaf( ) {
            ServerInformation.Feature.Deaf = true;
        }

        [BooleanFeature( "ETRACE" )]
        protected virtual void _handleETrace( ) {
            ServerInformation.Feature.ETrace = true;
        }

        [BooleanFeature( "EXCEPTS" )]
        protected virtual void _handleExcepts( ) {
            ServerInformation.Feature.BanExceptions = true;
        }

        [BooleanFeature( "FNC" )]
        protected virtual void _handleFnc( ) {
            ServerInformation.Feature.ForcedNickChanges = true;
        }

        [BooleanFeature( "HCN" )]
        protected virtual void _handleHcn( ) {
            ServerInformation.Feature.Hcn = true;
        }

        [BooleanFeature( "INVEX" )]
        protected virtual void _handleInvEx( ) {
            ServerInformation.Feature.InviteExceptions = true;
        }

        [BooleanFeature( "KNOCK" )]
        protected virtual void _handleKnock( ) {
            ServerInformation.Feature.Knock = true;
        }

        [BooleanFeature( "NAMESX" )]
        protected virtual void _handleNamesX( ) {
            ServerInformation.Feature.MultipleStatusesInNames = true;
        }

        [BooleanFeature( "NOQUIT" )]
        protected virtual void _handleNoQuit( ) {
            ServerInformation.Feature.NoQuit = true;
        }

        [BooleanFeature( "PENALTY" )]
        protected virtual void _handlePenalty( ) {
            ServerInformation.Feature.Penalty = true;
        }

        [BooleanFeature( "RFC2812" )]
        protected virtual void _handleRfc2812( ) {
            _handleStd( "RFC2812" );
        }

        [BooleanFeature( "SAFELIST" )]
        protected virtual void _handleSafeList( ) {
            ServerInformation.Feature.SafeList = true;
        }

        [BooleanFeature( "UHNAMES" )]
        protected virtual void _handleUhNames( ) {
            ServerInformation.Feature.UserHostInNames = true;
        }

        [BooleanFeature( "USERIP" )]
        protected virtual void _handleUserIp( ) {
            ServerInformation.Feature.UserIp = true;
        }

        [BooleanFeature( "USRIP" )]
        protected virtual void _handleUsrIp( ) {
            ServerInformation.Feature.UsrIp = true;
        }

        [BooleanFeature( "VCHANS" )]
        protected virtual void _handleVChans( ) {
            ServerInformation.Feature.VChans = true;
        }

        [BooleanFeature( "WALLCHOPS" )]
        protected virtual void _handleWallChops( ) {
            ServerInformation.Feature.WallChops = true;
        }

        [BooleanFeature( "WALLVOICES" )]
        protected virtual void _handleWallVoices( ) {
            ServerInformation.Feature.WallVoices = true;
        }

        [BooleanFeature( "WHOX" )]
        protected virtual void _handleWhoX( ) {
            ServerInformation.Feature.WhoX = true;
        }

        // -- String features

        [StringFeature( "AWAYLEN" )]
        protected virtual void _handleAwayLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.AwayReasonLength = _ );
        }

        [StringFeature( "CALLERID" )]
        protected virtual void _handleCallerId( string value ) {
            ServerInformation.Feature.CallerId = true;
            ServerInformation.UserModes.CallerId = value;
        }

        [StringFeature( "CASEMAPPING" )]
        protected virtual void _handleCaseMapping( string value ) {
            ServerInformation.CaseMapping = value;
        }

        [StringFeature( "CHANLIMIT" )]
        protected virtual void _handleChanLimit( string value ) {
            var z1 = ListHelper.ParseList( value );
            var z2 = ListHelper.ParseValuesAsInt( z1 );
            var z3 = ListHelper.ExplodeKeys( z2 );
            ListHelper.AddListToDictionary( z3, ServerInformation.Limit.Channels );
        }

        [StringFeature( "CHANMODES" )]
        protected virtual void _handleChanModes( string value ) {
            var parts = value.Split( CommonDelimiters.Comma, 5, StringSplitOptions.None );
            ServerInformation.ChannelModes = new ServerInformation.ChannelModesInfo {
                TypeA = parts[ 0 ],
                TypeB = parts[ 1 ],
                TypeC = parts[ 2 ],
                TypeD = parts[ 3 ],
            };
        }

        [StringFeature( "CHANNELLEN" )]
        protected virtual void _handleChannelLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.ChannelNameLength = _ );
        }

        [StringFeature( "CHANTYPES" )]
        protected virtual void _handleChanTypes( string value ) {
            ServerInformation.ChannelTypes = value;
        }

        [StringFeature( "CHARSET" )]
        protected virtual void _handleCharSet( string value ) {
            ServerInformation.CharacterSet = value;
        }

        [StringFeature( "CHIDLEN" )]
        protected virtual void _handleChIdLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.SafeChannelIdPrefixLength.ReplaceOrAdd( '!', _ ) );
        }

        [StringFeature( "CLIENTVER" )]
        protected virtual void _handleClientVer( string value ) {
            ServerInformation.ClientVersion = value;
        }

        [StringFeature( "CMDS" )]
        protected virtual void _handleCmds( string value ) {

        }

        [StringFeature( "DEAF" )]
        protected virtual void _handleDeaf( string value ) {
            ServerInformation.Feature.Deaf = true;
            ServerInformation.UserModes.Deaf = value;
        }

        [StringFeature( "ELIST" )]
        protected virtual void _handleEList( string value ) {
            var elist = ListSearchExtensionTypes.None;
            foreach ( var ch in value ) {
                if ( EListMap.ContainsKey( ch ) ) {
                    elist |= EListMap[ ch ];
                }
            }
            ServerInformation.Feature.ListSearchExtensions = new ServerInformation.ListSearchExtensionsInfo( elist );
        }

        [StringFeature( "EXCEPTS" )]
        protected virtual void _handleExcepts( string value ) {
            ServerInformation.Feature.BanExceptions = true;
            ServerInformation.ChannelModes.BanExceptions = value;
        }

        [StringFeature( "EXTBAN" )]
        protected virtual void _handleExtBan( string value ) {

        }

        [StringFeature( "IDCHAN" )]
        protected virtual void _handleIdChan( string value ) {
            var z1 = ListHelper.ParseList( value );
            var z2 = ListHelper.ParseValuesAsInt( z1 );
            var z3 = ListHelper.ExplodeKeys( z2 );
            ListHelper.AddListToDictionary( z3, ServerInformation.Limit.SafeChannelIdPrefixLength );
        }

        [StringFeature( "INVEX" )]
        protected virtual void _handleInvEx( string value ) {
            ServerInformation.Feature.InviteExceptions = true;
            ServerInformation.ChannelModes.InviteExceptions = value;
        }

        [StringFeature( "KICKLEN" )]
        protected virtual void _handleKickLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.KickCommentLength = _ );
        }

        [StringFeature( "MAXBANS" )]
        protected virtual void _handleMaxBans( string value ) {
            IntHelper.TryParse( value, _ => {
                foreach ( var key in ServerInformation.ChannelModes.TypeA ) {
                    ServerInformation.Limit.TypeAModeListEntries[ key ] = _;
                }
            } );
        }

        [StringFeature( "MAXCHANNELLEN" )]
        protected virtual void _handleMaxChannelLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.MaximumChannelNameLength = _ );
        }

        [StringFeature( "MAXCHANNELS" )]
        protected virtual void _handleMaxChannels( string value ) {
            IntHelper.TryParse( value, _ => {
                foreach ( var key in ServerInformation.ChannelTypes ) {
                    ServerInformation.Limit.Channels[ key ] = _;
                }
                ServerInformation.Limit.TotalChannels = _;
            } );
        }

        [StringFeature( "MAXLIST" )]
        protected virtual void _handleMaxList( string value ) {
            var z1 = ListHelper.ParseList( value );
            var z2 = ListHelper.ParseValuesAsInt( z1 );
            var z3 = ListHelper.ExplodeKeys( z2 );
            ListHelper.AddListToDictionary( z3, ServerInformation.Limit.TypeAModeListEntries );
        }

        [StringFeature( "MAXNICKLEN" )]
        protected virtual void _handleMaxNickLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.MaximumNickNameLength = _ );
        }

        [StringFeature( "MAXTARGETS" )]
        protected virtual void _handleMaxTargets( string value ) {

        }

        [StringFeature( "MODES" )]
        protected virtual void _handleModes( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.TypeBModesPerCommand = _ );
        }

        [StringFeature( "MONITOR" )]
        protected virtual void _handleMonitor( string value ) {

        }

        [StringFeature( "NETWORK" )]
        protected virtual void _handleNetwork( string value ) {
            ServerInformation.Network = value;
        }

        [StringFeature( "NICKLEN" )]
        protected virtual void _handleNickLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.NickNameLength = _ );
        }

        [StringFeature( "PREFIX" )]
        protected virtual void _handlePrefix( string value ) {
            var m = PrefixParserRegex.Match( value );
            if ( !m.Success ) {
                return;
            }

            ServerInformation.ChannelModes.Statuses = m.Groups[ 1 ].Value;
            ServerInformation.ChannelModes.StatusSigils = m.Groups[ 2 ].Value;

            for ( var index = 0; index < Math.Min( ServerInformation.ChannelModes.Statuses.Length, ServerInformation.ChannelModes.StatusSigils.Length ); index++ ) {
                ServerInformation.ChannelModes.StatusSigilToMode[ ServerInformation.ChannelModes.StatusSigils[ index ] ] = ServerInformation.ChannelModes.Statuses[ index ];
                ServerInformation.ChannelModes.StatusModeToSigil[ ServerInformation.ChannelModes.Statuses[ index ] ] = ServerInformation.ChannelModes.StatusSigils[ index ];
            }
        }

        [StringFeature( "SILENCE" )]
        protected virtual void _handleSilence( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.SilenceEntries = _ );
        }

        [StringFeature( "STATUSMSG" )]
        protected virtual void _handleStatusMsg( string value ) {

        }

        [StringFeature( "STD" )]
        protected virtual void _handleStd( string value ) {
            ServerInformation.ProtocolStandardString = value;

            try {
                if ( string.IsNullOrWhiteSpace( value ) ) {
                    ServerInformation.ProtocolStandard = ProtocolStandard.Unknown;
                }
                var obj = Enum.Parse( typeof( ProtocolStandard ), value ?? string.Empty, true );
                ServerInformation.ProtocolStandard = (ProtocolStandard) obj;
            }
            catch ( Exception e ) {
                Debug.Print( "ISupportHandler._handleStd: caught exception parsing '{0}':\n{1}", value, e );
                ServerInformation.ProtocolStandard = ProtocolStandard.Other;
            }
        }

        [StringFeature( "TARGMAX" )]
        protected virtual void _handleTargMax( string value ) {

        }

        [StringFeature( "TOPICLEN" )]
        protected virtual void _handleTopicLen( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.TopicLength = _ );
        }

        [StringFeature( "WATCH" )]
        protected virtual void _handleWatch( string value ) {
            IntHelper.TryParse( value, _ => ServerInformation.Limit.WatchEntries = _ );
        }

        [StringFeature( "WATCHOPTS" )]
        protected virtual void _handleWatchOpts( string value ) {

        }

    }

}
