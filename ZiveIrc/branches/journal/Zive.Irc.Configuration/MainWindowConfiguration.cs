using System.ComponentModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class MainWindowConfiguration {

        public MainWindowConfiguration( ) {
            ShowConsolePanelOnStartup = true;

            SavedLeft = -1;
            SavedTop = -1;
            SavedWidth = -1;
            SavedHeight = -1;
            SavedState = -1;
        }

        [DefaultValue( true )]
        public bool ShowConsolePanelOnStartup { get; set; }

        [DefaultValue( -1 )]
        public int SavedLeft;

        [DefaultValue( -1 )]
        public int SavedTop;

        [DefaultValue( -1 )]
        public int SavedWidth;

        [DefaultValue( -1 )]
        public int SavedHeight;

        [DefaultValue( -1 )]
        public int SavedState;

    }

}
