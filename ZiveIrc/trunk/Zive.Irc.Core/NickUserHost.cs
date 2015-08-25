#undef NUHDEBUG

using System.Diagnostics;
using System.Text.RegularExpressions;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class NickUserHost {

        //
        // Interface
        //

        // Properties

        public bool IsEmpty { get { return _empty; } }

        public string NickName {
            get { return _nickName; }
            set {
                _nickName = value;
                _CheckEmpty( );
                _SetNickUserHost( );
            }
        }

        public string UserName {
            get { return _userName; }
            set {
                _userName = value;
                _CheckEmpty( );
                _SetUserHost( );
            }
        }

        public string HostName {
            get { return _hostName; }
            set {
                _hostName = value;
                _CheckEmpty( );
                _SetUserHost( );
            }
        }

        public string UserHost { get { return _userHost; } }

        // Events

        // Constructors

        private static readonly Regex NickUserHostMatcher = new Regex( "^(?:(.*?)!|)(.*?)(?:@(.*?)|)$" );

        public NickUserHost( ) {
            Clear( );
        }

        public NickUserHost( string nickUserHost ): this( ) {
            Set( nickUserHost );
        }

        // Methods

        public void Set( string nickUserHost ) {
            if ( string.IsNullOrWhiteSpace( nickUserHost ) ) {
                return;
            }

            if ( -1 == nickUserHost.IndexOfAny( CommonDelimiters.BangAndAt ) ) {
                if ( -1 == nickUserHost.IndexOf( '.' ) ) {
                    _nickName = nickUserHost;
                } else {
                    _hostName = nickUserHost;
                }
            } else {
                var m = NickUserHostMatcher.Match( nickUserHost );
                if ( !string.IsNullOrWhiteSpace( m.Groups[ 1 ].Value ) ) {
                    _nickName = m.Groups[ 1 ].Value;
                }
                if ( !string.IsNullOrWhiteSpace( m.Groups[ 2 ].Value ) ) {
                    _userName = m.Groups[ 2 ].Value;
                }
                if ( !string.IsNullOrWhiteSpace( m.Groups[ 3 ].Value ) ) {
                    _hostName = m.Groups[ 3 ].Value;
                }
            }
            _CheckEmpty( );
            _SetUserHost( );
        }

        public void Clear( ) {
            _nickName = string.Empty;
            _userName = string.Empty;
            _hostName = string.Empty;

            _userHost = string.Empty;
            _nickUserHost = string.Empty;

            _empty = true;

            _nickEmpty = true;
            _userEmpty = true;
            _hostEmpty = true;

            _userHostEmpty = true;
        }

        public override string ToString( ) {
            return _nickUserHost;
        }

        public static explicit operator string( NickUserHost self ) {
            return self._nickUserHost;
        }

        //
        // Implementation
        //

        private static readonly BooleanSwitch DebugNickUserHost = new BooleanSwitch( "DebugNickUserHost", "Controls whether debugging output from class NickUserHost is emitted to the debugging log.", "0" );

        private string _nickName;
        private string _userName;
        private string _hostName;

        private string _userHost;
        private string _nickUserHost;

        private bool _empty;
        private bool _nickEmpty;
        private bool _userEmpty;
        private bool _hostEmpty;
        private bool _userHostEmpty;

        private void _debug( string format, params object[ ] args ) {
            if ( DebugNickUserHost.Enabled ) {
                Debug.Print( format, args );
            }
        }

        private void _CheckEmpty( ) {
            _nickEmpty = string.IsNullOrWhiteSpace( _nickName );
            _userEmpty = string.IsNullOrWhiteSpace( _userName );
            _hostEmpty = string.IsNullOrWhiteSpace( _hostName );
            _empty = _nickEmpty && _userEmpty && _hostEmpty;
        }

        private void _SetUserHost( ) {
            if ( _userEmpty && !_hostEmpty ) {
                _userHost = _hostName;
                _userHostEmpty = false;
                _debug( "$$ SUH1 {0}{1}{2} '{3}'", _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _userHost );
            } else if ( !_userEmpty && !_hostEmpty ) {
                _userHost = _userName + "@" + _hostName;
                _userHostEmpty = false;
                _debug( "$$ SUH2 {0}{1}{2} '{3}'", _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _userHost );
            } else {
                _userHost = string.Empty;
                _userHostEmpty = true;
                _debug( "$$ SUHx {0}{1}{2} '{3}'", _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _userHost );
            }
            _SetNickUserHost( );
        }

        private void _SetNickUserHost( ) {
            if ( !_nickEmpty && _userHostEmpty ) {
                _nickUserHost = _nickName;
                _debug( "$$ SNUH1 {0}{1}{2}{3} '{4}'", _nickEmpty ? 'T' : 'f', _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _nickUserHost );
            } else if ( !_nickEmpty && !_userHostEmpty ) {
                _nickUserHost = _nickName + "!" + _userHost;
                _debug( "$$ SNUH2 {0}{1}{2}{3} '{4}'", _nickEmpty ? 'T' : 'f', _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _nickUserHost );
            } else if ( _nickEmpty && !_userHostEmpty ) {
                _nickUserHost = _userHost;
                _debug( "$$ SNUH3 {0}{1}{2}{3} '{4}'", _nickEmpty ? 'T' : 'f', _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _nickUserHost );
            } else {
                _nickUserHost = string.Empty;
                _debug( "$$ SNUHx {0}{1}{2}{3} '{4}'", _nickEmpty ? 'T' : 'f', _userEmpty ? 'T' : 'f', _hostEmpty ? 'T' : 'f', _userHostEmpty ? 'T' : 'f', _nickUserHost );
            }
        }

    }

}
