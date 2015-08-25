using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    public class ConfigurationManager {

        private readonly XmlSerializer _serializer = new XmlSerializer( typeof( ConfigurationRoot ), "http://www.zive.ca/xml/ns/ziveirc/configuration/1.0" );

        public ConfigurationRoot Configuration { get; set; }

        public void Load( string configFileName ) {
            RefIdManager.Reset( );

            try {
                using ( TextReader reader = new StreamReader( configFileName ) ) {
                    Configuration = (ConfigurationRoot) _serializer.Deserialize( reader );
                }
            }
            catch ( Exception e ) {
                Debug.Print( "ConfigurationManager.Load: caught exception reading '{0}':\n{1}", configFileName, e );
                throw;
            }
            Debug.Print( "ConfigurationManager.Load: RefIdManager.Peek() [2] returns {0}", RefIdManager.Peek( ) );
        }

        public void Save( string configFileName ) {
            try {
                using ( TextWriter writer = new StreamWriter( configFileName ) ) {
                    _serializer.Serialize( writer, Configuration );
                }
            }
            catch ( Exception e ) {
                Debug.Print( "ConfigurationManager.Save: caught exception writing '{0}':\n{1}", configFileName, e );
                throw;
            }
        }

    }

}
