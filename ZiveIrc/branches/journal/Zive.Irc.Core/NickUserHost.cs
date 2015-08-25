using System;
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
        public bool IsNickEmpty { get { return _nickEmpty; } }
        public bool IsUserEmpty { get { return _userEmpty; } }
        public bool IsHostEmpty { get { return _hostEmpty; } }
        public bool IsServerName { get { return _nickEmpty && _userEmpty && !_hostEmpty; } }

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

        }

        public NickUserHost( string nickUserHost ) : this( ) {
            Set( nickUserHost );
        }

        public NickUserHost( string nick, string user, string host ) : this( ) {
            _nickName = nick;
            _userName = user;
            _hostName = host;
            _CheckEmpty( );
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
                if ( m.Success ) {
                    if ( !string.IsNullOrWhiteSpace( m.Groups[1].Value ) ) {
                        _nickName = m.Groups[1].Value;
                    }
                    if ( !string.IsNullOrWhiteSpace( m.Groups[2].Value ) ) {
                        _userName = m.Groups[2].Value;
                    }
                    if ( !string.IsNullOrWhiteSpace( m.Groups[3].Value ) ) {
                        _hostName = m.Groups[3].Value;
                    }
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

        //
        // Object
        //

        public override bool Equals( object rhs ) {
            NickUserHost value = rhs as NickUserHost;
            if ( null == value ) {
                return ( rhs is string ) && string.Equals( _nickUserHost, (string) rhs, StringComparison.OrdinalIgnoreCase );
            }

            return
                string.Equals( _nickName, value._nickName, StringComparison.OrdinalIgnoreCase ) &&
                string.Equals( _userName, value._userName, StringComparison.OrdinalIgnoreCase ) &&
                string.Equals( _hostName, value._hostName, StringComparison.OrdinalIgnoreCase );
        }

        public override int GetHashCode( ) {
            return _nickName.GetHashCode( ) ^ _userName.GetHashCode( ) ^ _hostName.GetHashCode( );
        }

        public override string ToString( ) {
            return _nickUserHost;
        }

        //
        // Implementation
        //

        private static readonly BooleanSwitch DebugNickUserHost = new BooleanSwitch( "DebugNickUserHost", "Controls whether debugging output from class NickUserHost is emitted to the debugging log.", "0" );

        private string _nickName = string.Empty;
        private string _userName = string.Empty;
        private string _hostName = string.Empty;

        private string _userHost = string.Empty;
        private string _nickUserHost = string.Empty;

        private bool _empty = true;
        private bool _nickEmpty = true;
        private bool _userEmpty = true;
        private bool _hostEmpty = true;
        private bool _userHostEmpty = true;

        [Conditional("DEBUG")]
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
