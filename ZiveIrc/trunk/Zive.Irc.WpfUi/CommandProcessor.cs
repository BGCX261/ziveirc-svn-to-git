using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zive.Irc.Configuration;

namespace Zive.Irc.WpfUi {

    public class CommandInformation {

        public string Name;
        public Func<string, bool> HandlerFunc;

    }

    public class CommandProcessor {

        //
        // Interface
        //

        // Fields

        public Dictionary<string, CommandInformation> Commands;

        // Properties

        // Events

        // Constructors

        public CommandProcessor( ) {
            Commands = new Dictionary<string, CommandInformation> {
                {
                    "test",
                    new CommandInformation {
                        Name = "test",
                        HandlerFunc = TestCommand,
                    }
                },
            };
        }

        // Methods

        public bool Process( string input ) {
            if ( input[ 0 ] != _configuration.GeneralConfiguration.CommandCharacter ) {
                return false;
            }

            int index = 1;
            int endOfCommand = -1;
            string command;
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
                input = input.Substring( endOfCommand + 2 );
            }

            Debug.Print( "CommandProcessor.Process:" );
            Debug.Print( "+ command='{0}'", command );
            Debug.Print( "+ rest='{0}'", input );

            if ( !Commands.ContainsKey( command ) ) {
                Debug.Print( "+ Command not found in command dictionary." );
                return false;
            }

            var handler = Commands[ command ].HandlerFunc;
            if ( null == handler ) {
                Debug.Print( "+ Command's HandlerFunc field is null." );
                return false;
            }

            bool result;
            try {
                result = handler( input );
            }
            catch ( Exception e ) {
                Debug.Print( "+ caught exception during handler resolution and invocation:\n{0}", e );
                return false;
            }
            Debug.Print( "+ result from HandlerFunc: {0}", result ? "TRUE" : "false" );

            return true;
        }

        //
        // Implementation
        //

        // Fields

        private readonly ConfigurationRoot _configuration = App.Configuration;

        // Properties

        // Events

        // Constructors

        // Methods

        private bool TestCommand( string args ) {
            Debug.Print( "CommandProcessor.TestCommand: args='{0}'", args );
            return true;
        }

    }

}
