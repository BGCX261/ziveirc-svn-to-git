using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Zive.Irc.WpfUi {

    public enum ScrollAction {
        LineUp,
        LineDown,
        PageUp,
        PageDown,
        Top,
        Bottom,
    }

    public enum HistoryAction {
        LineUp,
        LineDown,
    }

    public partial class InputBar: UserControl {

        //
        // Interface
        //

        // Properties

        public string SelectedText    { get { return TextInputArea.SelectedText;    } set { TextInputArea.SelectedText    = value; } }
        public int    SelectionLength { get { return TextInputArea.SelectionLength; } set { TextInputArea.SelectionLength = value; } }
        public int    SelectionStart  { get { return TextInputArea.SelectionStart;  } set { TextInputArea.SelectionStart  = value; } }
        public string Text            { get { return TextInputArea.Text;            } set { TextInputArea.Text            = value; } }

        public Collection<string> InputHistory { get { return _inputHistory; } }

        // Events

        public event EventHandler<LineInputEventArgs>     LineInput;
        public event EventHandler<RelayedScrollEventArgs> RelayedScroll;

        // Constructors

        public InputBar( ) {
            _historyActions = new Dictionary<HistoryAction, Action<ExecutedRoutedEventArgs>> {
                { HistoryAction.LineUp,   _HistoryLineUp   },
                { HistoryAction.LineDown, _HistoryLineDown },
            };

            InitializeComponent( );
        }

        // Methods

        //
        // Implementation
        //

        // Fields

        private readonly Collection<string> _inputHistory = new Collection<string>( );
        private readonly Dictionary<HistoryAction, Action<ExecutedRoutedEventArgs>> _historyActions;
        private int _currentHistoryIndex = -1;

        // Properties

        // Events

        // Constructors

        // Methods

        private void _HistoryLineUp( ExecutedRoutedEventArgs ev ) {
            Debug.Print( "InputBar._HistoryLineUp: _currentHistoryIndex={0}", _currentHistoryIndex );
            if ( -1 == _currentHistoryIndex ) {
                Debug.Print( "+ starting history browse" );
                if ( !string.IsNullOrEmpty( Text ) ) {
                    Debug.Print( "+ stashing current input line" );
                    InputHistory.Add( Text );
                }
                _currentHistoryIndex = InputHistory.Count - 1;
            } else {
                Debug.Print( "+ continuing history browse" );
                _currentHistoryIndex = Math.Max( _currentHistoryIndex - 1, 0 );
            }

            Debug.Print( "+ new _currentHistoryIndex: {0}", _currentHistoryIndex );
            Text = InputHistory[ _currentHistoryIndex ];
        }

        private void _HistoryLineDown( ExecutedRoutedEventArgs ev ) {
            Debug.Print( "InputBar._HistoryLineDown: _currentHistoryIndex={0}", _currentHistoryIndex );
            if ( -1 == _currentHistoryIndex ) {
                Debug.Print( "+ browse not in progress, aborting" );
                return;
            }
            if ( _currentHistoryIndex == InputHistory.Count - 1 ) {
                Debug.Print( "+ already at end of history, aborting" );
                return;
            }
            _currentHistoryIndex++;

            Debug.Print( "+ new _currentHistoryIndex: {0}", _currentHistoryIndex );
            Text = InputHistory[ _currentHistoryIndex ];
        }

        // Polyvalent event handlers

        private void RoutedEvent_CanExecuteAlways( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = true;
        }

        private void RoutedEvent_GotFocus( object sender, RoutedEventArgs ev ) {
            TextInputArea.Focus( );
        }

        // Command handlers

        // HistoryAction
        private void HistoryAction_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( InputHistory.Count > 0 );
        }

        private void HistoryAction_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            _historyActions[ (HistoryAction) ev.Parameter ]( ev );
        }

        // LineInput
        private void LineInput_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( Text.Length > 0 );
        }

        private void LineInput_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            var input = Text;

            InputHistory.Add( input );
            _currentHistoryIndex = -1;

            if ( OnLineInput( input ) ) {
                TextInputArea.Clear( );
            }
        }

        // ScrollAction
        private void ScrollAction_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            OnScroll( (ScrollAction) ev.Parameter );
        }

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

        private void OnScroll( ScrollAction scrollAction ) {
            var handler = RelayedScroll;
            if ( null != handler ) {
                handler( this, new RelayedScrollEventArgs( scrollAction ) );
            }
        }

    }

}
