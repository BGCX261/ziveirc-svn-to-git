using System.ComponentModel;
#if DEBUG
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class DebuggingConfiguration {

        public DebuggingConfiguration( ) {
#if DEBUG
            ShowDebuggingOutputInConsoleTab = true;
#else
            ShowDebuggingOutputInConsoleTab = false;
#endif
        }

#if DEBUG
        [DefaultValue( true )]
#else
        [DefaultValue( false )]
#endif
        public bool ShowDebuggingOutputInConsoleTab { get; set; }

    }

}
#endif
