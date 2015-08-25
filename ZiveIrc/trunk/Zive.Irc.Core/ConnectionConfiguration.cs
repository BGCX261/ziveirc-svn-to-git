using System.Collections.ObjectModel;

namespace Zive.Irc.Core {

    public class ConnectionConfiguration {

        public string Password { get; set; }
        public string NickName { get; set; }
        public string UserName { get; set; }
        public string LocalHostName { get; set; }
        public string RealName { get; set; }

        public string NickServUserName { get; set; }
        public string NickServPassword { get; set; }

        public string ServerHostName { get; set; }
        public Collection<int> Ports { get; set; }

    }

}
