using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    public class ConfigurationManager {

        private const string ConfigurationNameSpace = "http://www.zive.ca/xml/ns/ziveirc/configuration/1.0";

        public ConfigurationRoot Configuration { get; set; }

        public void Load( string configFileName ) {
            RefIdManager.Reset( );

            Debug.Print( "ConfigurationManager.Load: Trying to read configuration file '{0}'", configFileName );
            try {
                using ( TextReader reader = new StreamReader( configFileName ) ) {
                    var serializer = new XmlSerializer( typeof(ConfigurationRoot), ConfigurationNameSpace );
                    Configuration = (ConfigurationRoot) serializer.Deserialize( reader );
                }
            }
            catch ( Exception e ) {
                Debug.Print( "ConfigurationManager.Load: caught exception reading '{0}':\n{1}", configFileName, e );
                throw;
            }
            Debug.Print( "ConfigurationManager.Load: RefIdManager.Peek() returns {0}", RefIdManager.Peek( ) );
        }

        public void Save( string configFileName ) {
            Debug.Print( "ConfigurationManager.Save: Trying to write configuration file '{0}'", configFileName );
            try {
                using ( TextWriter writer = new StreamWriter( configFileName ) ) {
                    var serializer = new XmlSerializer( typeof(ConfigurationRoot), ConfigurationNameSpace );
                    serializer.Serialize( writer, Configuration );
                }
            }
            catch ( Exception e ) {
                Debug.Print( "ConfigurationManager.Save: caught exception writing '{0}':\n{1}", configFileName, e );
                throw;
            }
        }

    }

}
