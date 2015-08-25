using System.ComponentModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class UserConfiguration {

        public UserConfiguration( ) {
            QuitMessage = "changing universes";
        }

        public string NickName = string.Empty;
        public string UserName = string.Empty;
        public string RealName = string.Empty;

        [DefaultValue("changing universes")]
        public string QuitMessage { get; set; }

    }

}
