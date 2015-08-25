using System;

namespace Zive.Irc.Core {

    public enum ChannelModeType {
        Unknown,
        TypeA,
        TypeB,
        TypeC,
        TypeD,
    }

    public enum ChannelVisibility {
        Unknown,
        Public,
        Secret,
        Other
    }

    [Flags]
    public enum ListSearchExtensionTypes {
        None         =  0,
        Mask         =  1,
        NotMask      =  2,
        UserCount    =  4,
        CreationTime =  8,
        Topic        = 16,
    }

    public enum ProtocolStandard {
        Unknown,
        Rfc1459,
        Rfc2812,
        Other,
    }

    public class WhoResponse {

        public string ChannelName { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string ServerName { get; set; }
        public string NickName { get; set; }
        public string Flags { get; set; }
        public int HopCount { get; set; }
        public string RealName { get; set; }
        public User User { get; set; }

    }

}
