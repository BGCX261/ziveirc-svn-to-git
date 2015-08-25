using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Zive.Irc.Configuration {

    [XmlType]
    public class GeneralConfiguration {

        public GeneralConfiguration( ) {
            CommandCharacter = '/';
        }

        [DefaultValue( '/' )]
        public char CommandCharacter { get; set; }

    }

}
