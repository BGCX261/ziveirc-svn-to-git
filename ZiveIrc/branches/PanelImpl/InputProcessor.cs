using System;
using System.Windows;

namespace Zive.Irc.WpfUi {

    public class InputProcessor {

        //
        // Interface
        //

        // Fields

        // Properties

        public CommandProcessor CommandProcessor { get; set; }

        // Events

        public event EventHandler<LineInputEventArgs> LineInput;

        // Constructors

        public InputProcessor( ) {
            _configuration = _app.ConfigurationManager.Configuration;
        }

        // Methods

        public bool Process( string input ) {
            if ( string.IsNullOrEmpty( input ) ) {
                return false;
            }

            if ( input[ 0 ] == _configuration.GeneralConfiguration.CommandCharacter ) {
                return CommandProcessor.Process( input );
            }

            return OnLineInput( input );
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

        // Event dispatchers

        private bool OnLineInput( string line ) {
            var handler = LineInput;
            if ( null != handler ) {
                var ev = new LineInputEventArgs( line );
                handler( this, ev );
                return ev.AllowClear;
            }
            return true;
        }

    }

}
