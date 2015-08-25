using System.Collections.Generic;

namespace Zive.Irc.Core {

    /*
     * Boolean Feature Keys
     * =======================
     * ACCEPT                  ✓     [2]
     * CALLERID                ✓     [2]
     * CNOTICE                 ✓     [2]
     * CPRIVMSG                ✓     [2]
     * DEAF                    ✓
     * ETRACE                  ✓
     * EXCEPTS                 ✓ [1]
     * FNC                     ✓     [2]
     * HCN                     ✓
     * INVEX                   ✓ [1]
     * KNOCK                   ✓     [2]
     * NAMESX                             [3]
     * NOQUIT                  ✓     [2]
     * PENALTY                 ✓     [2]
     * RFC2812                 ✓     [2]
     * SAFELIST                ✓ [1] [2]
     * UHNAMES                 ✓         [3]
     * USERIP                  ✓     [2]
     * USRIP                   ✓
     * VCHANS                  ✓     [2]
     * WALLCHOPS               ✓     [2]
     * WALLVOICES              ✓     [2]
     * WHOX                    ✓     [2]
     *
     * String Feature Keys
     * =======================
     * AWAYLEN                 ✓      [2]
     * CALLERID                ✓
     * CASEMAPPING             ✓  [1] [2]
     * CHANLIMIT               ✓  [1] [2]
     * CHANMODES               ✓  [1] [2]
     * CHANNELLEN              ✓  [1] [2]
     * CHANTYPES               ✓  [1] [2]
     * CHARSET                 ✓
     * CHIDLEN                 ✓      [2]
     * CLIENTVER               ✓
     * CMDS                     ✗
     * DEAF                    ✓
     * ELIST                   ✓      [2]
     * EXCEPTS                 ✓  [1] [2]
     * EXTBAN                   ✗
     * IDCHAN                  ✓  [1] [2]
     * INVEX                   ✓  [1] [2]
     * KICKLEN                 ✓  [1] [2]
     * MAXBANS                 ✓      [2]
     * MAXCHANNELLEN           ✓
     * MAXCHANNELS             ✓      [2]
     * MAXLIST                 ✓  [1] [2]
     * MAXNICKLEN              ✓      [2]
     * MAXTARGETS               ✗     [2]
     * MODES                   ✓  [1] [2]
     * MONITOR                  ✗         [3]
     * NETWORK                 ✓  [1] [2]
     * NICKLEN                 ✓  [1] [2]
     * PREFIX                  ✓  [1] [2]
     * SILENCE                 ✓      [2]
     * STATUSMSG                ✗ [1] [2]
     * STD                     ✓  [1] [2]
     * TARGMAX                  ✗ [1]
     * TOPICLEN                ✓  [1] [2]
     * WATCH                   ✓      [2]
     * WATCHOPTS                ✗
     *
     * [1] http://www.irc.org/tech_docs/draft-brocklesby-irc-isupport-03.txt
     * [2] http://www.irc.org/tech_docs/005.html
     * [3] http://www.mirc.co.uk/versions.txt
    */

    // ReSharper disable InconsistentNaming

    public class ServerInformation {

        //
        // Interface
        //

        // Types

        public class Limits {

            public Limits( ) {
                NickNameLength = 9;
                MaximumNickNameLength = 9;
                ChannelNameLength = 200;
                MaximumChannelNameLength = 200;
                TopicLength = 307;
                KickCommentLength = 307;
                AwayReasonLength = 307;

                WatchEntries = 0;
                SilenceEntries = 0;

                TypeBModesPerCommand = 3;

                TotalChannels = 10;

                Channels = new Dictionary<char, int>( );
                TypeAModeListEntries = new Dictionary<char, int>( );
            }

            public int NickNameLength { get; set; }
            public int MaximumNickNameLength { get; set; }
            public int ChannelNameLength { get; set; }
            public int MaximumChannelNameLength { get; set; }
            public int TopicLength { get; set; }
            public int KickCommentLength { get; set; }
            public int AwayReasonLength { get; set; }

            public int WatchEntries { get; set; }
            public int SilenceEntries { get; set; }

            public int TypeBModesPerCommand { get; set; }

            public int TotalChannels { get; set; }

            public Dictionary<char, int> Channels { get; private set; }
            public Dictionary<char, int> TypeAModeListEntries { get; private set; }
            public Dictionary<char, int> SafeChannelIdPrefixLength { get; private set; }

        }

        public class ChannelModesInfo {

            public ChannelModesInfo( ) {
                TypeA = "bIe";
                TypeB = "k";
                TypeC = "l";
                TypeD = "imnpst";

                StatusSigilToMode = new Dictionary<char, char> {
                    { 'o', '@' },
                    { 'h', '%' },
                    { 'v', '+' },
                };
                StatusModeToSigil = new Dictionary<char, char> {
                    { '@', 'o' },
                    { '%', 'h' },
                    { '+', 'v' },
                };
                StatusSigils = "@%+";
                Statuses = "ohv";
            }

            public string TypeA { get; set; }
            public string TypeB { get; set; }
            public string TypeC { get; set; }
            public string TypeD { get; set; }

            public string BanExceptions { get; set; }
            public string InviteExceptions { get; set; }

            public Dictionary<char, char> StatusSigilToMode { get; private set; }
            public Dictionary<char, char> StatusModeToSigil { get; private set; }
            public string StatusSigils { get; set; }
            public string Statuses { get; set; }

            public ChannelModeType GetModeType( char mode ) {
                if ( TypeA.IndexOf( mode ) > -1 ) {
                    return ChannelModeType.TypeA;
                }
                if ( TypeB.IndexOf( mode ) > -1 ) {
                    return ChannelModeType.TypeB;
                }
                if ( Statuses.IndexOf( mode ) > -1 ) {
                    return ChannelModeType.TypeB;
                }
                if ( TypeC.IndexOf( mode ) > -1 ) {
                    return ChannelModeType.TypeC;
                }
                if ( TypeD.IndexOf( mode ) > -1 ) {
                    return ChannelModeType.TypeD;
                }
                return ChannelModeType.Unknown;
            }

        }

        public class UserModesInfo {

            public UserModesInfo( ) {
                All = "iosw";
            }

            public string All { get; set; }

            public string CallerId { get; set; }
            public string Deaf { get; set; }

        }

        public class ListSearchExtensionsInfo {

            public ListSearchExtensionsInfo( ) { }

            public ListSearchExtensionsInfo( ListSearchExtensionTypes value ) {
                Mask = 0 != ( value & ListSearchExtensionTypes.Mask );
                NotMask = 0 != ( value & ListSearchExtensionTypes.NotMask );
                UserCount = 0 != ( value & ListSearchExtensionTypes.UserCount );
                CreationTime = 0 != ( value & ListSearchExtensionTypes.CreationTime );
                Topic = 0 != ( value & ListSearchExtensionTypes.Topic );
            }

            public bool Mask { get; private set; }
            public bool NotMask { get; private set; }
            public bool UserCount { get; private set; }
            public bool CreationTime { get; private set; }
            public bool Topic { get; private set; }

        }

        public class Features {

            public Features( ) {
                ListSearchExtensions = new ListSearchExtensionsInfo( );
            }

            public bool CallerId { get; set; }
            public bool CNotice { get; set; }
            public bool CPrivMsg { get; set; }
            public bool Deaf { get; set; }
            public bool ETrace { get; set; }
            public bool BanExceptions { get; set; }
            public bool ForcedNickChanges { get; set; }
            public bool Hcn { get; set; }
            public bool InviteExceptions { get; set; }
            public bool Knock { get; set; }
            public bool MultipleStatusesInNames { get; set; }
            public bool NoQuit { get; set; }
            public bool Penalty { get; set; }
            public bool SafeList { get; set; }
            public bool ServerNoticeMask { get; set; }
            public bool UserHostInNames { get; set; }
            public bool UserIp { get; set; }
            public bool UsrIp { get; set; }
            public bool VChans { get; set; }
            public bool WallChops { get; set; }
            public bool WallVoices { get; set; }
            public bool WhoX { get; set; }

            public ListSearchExtensionsInfo ListSearchExtensions { get; set; }

        }

        // Properties

        public ChannelModesInfo ChannelModes { get; internal set; }
        public string CaseMapping { get; internal set; }
        public string ChannelTypes { get; internal set; }
        public string CharacterSet { get; internal set; }
        public string ClientVersion { get; internal set; }
        public Features Feature { get; internal set; }
        public Limits Limit { get; internal set; }
        public string Network { get; internal set; }
        public string ProtocolStandardString { get; internal set; }
        public ProtocolStandard ProtocolStandard { get; internal set; }
        public string ServerSoftware { get; internal set; }
        public UserModesInfo UserModes { get; internal set; }

        // Events

        // Constructors

        public ServerInformation( ) {
            ChannelModes = new ChannelModesInfo( );
            CaseMapping = "rfc1459";
            ChannelTypes = "#&";
            Feature = new Features( );
            Limit = new Limits( );
            UserModes = new UserModesInfo( );
        }

        // Methods

        //
        // Implementation
        //

        // Fields

        // Events

        // Constructors

        // Methods

    }

}
