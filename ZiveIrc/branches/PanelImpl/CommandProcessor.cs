using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Zive.Irc.WpfUi {

    public class CommandProcessor {

        //
        // Interface
        //

        // Fields

        // Properties

        // Events

        // Constructors

        public CommandProcessor( ) {
            _configuration = _app.ConfigurationManager.Configuration;
        }

        // Methods

        public bool Process( string input ) {
            if ( input[ 0 ] != _configuration.GeneralConfiguration.CommandCharacter ) {
                return false;
            }

            int index = 1;
            int endOfCommand = -1;
            string command = string.Empty;
            while ( index < input.Length ) {
                if ( char.IsWhiteSpace( input[ index ] ) ) {
                    endOfCommand = index - 1;
                    break;
                }
                index++;
            }
            if ( -1 == endOfCommand ) {
                command = input.Substring( 1 );
                input = string.Empty;
            } else {
                command = input.Substring( 1, endOfCommand );
                int startOfArgs = -1;
                while ( index < input.Length ) {
                    if ( !char.IsWhiteSpace( input[ index ] ) ) {
                        startOfArgs = index;
                        break;
                    }
                    index++;
                }
                input = ( -1 == startOfArgs ) ? string.Empty : input.Substring( startOfArgs );
            }

            Debug.Print( "CommandProcessor.Process:" );
            Debug.Print( "+ command='{0}'", command );
            Debug.Print( "+ rest='{0}'", input );

            return false;
        }

        //
        // Implementation
        //

        // Fields

        private readonly Configuration.Configuration _configuration;

        // Properties

        private App _app { get { return (App) Application.Current; } }

        // Events

        // Constructors

        // Methods

    }

}
