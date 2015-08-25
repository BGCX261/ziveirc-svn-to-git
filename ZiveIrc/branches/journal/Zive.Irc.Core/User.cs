using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Zive.Irc.Core.Annotations;

namespace Zive.Irc.Core {

    public class User: INotifyPropertyChanged, ITargetable {

        //
        // Interface
        //

        // Properties

        public virtual Server Server {
            get { return _server; }
            set {
                if ( null != _protocolHandler ) {
                    _protocolHandler.Unsubscribe( _messageFilters );
                }
                _server = value;
                if ( null != _server ) {
                    _protocolHandler = _server.ProtocolHandler;
                    _protocolHandler.Subscribe( _messageFilters );
                } else {
                    _protocolHandler = null;
                }
                OnPropertyChanged( );
            }
        }

        public virtual NickUserHost NickUserHost {
            get { return _nickUserHost; }
            set {
                _nickUserHost = value;
                OnPropertyChanged( );
            }
        }

        public virtual string NickName {
            get { return _nickUserHost.NickName; }
            set {
                string oldNick = _nickUserHost.NickName;
                _nickUserHost.NickName = value;
                OnNickChanged( oldNick, value );
                OnPropertyChanged( );
            }
        }

        public virtual string UserName {
            get { return _nickUserHost.UserName; }
            set {
                _nickUserHost.UserName = value;
                OnPropertyChanged( );
            }
        }

        public virtual string HostName {
            get { return _nickUserHost.HostName; }
            set {
                _nickUserHost.HostName = value;
                OnPropertyChanged( );
            }
        }

        public virtual string UserHost {
            get { return _nickUserHost.UserHost; }
        }

        public virtual string RealName {
            get { return _realName; }
            set {
                _realName = value;
                OnPropertyChanged( );
            }
        }
        public virtual string RealHostName {
            get { return _realHostName; }
            set {
                _realHostName = value;
                OnPropertyChanged( );
            }
        }

        public string UserMode {
            get { return string.IsNullOrWhiteSpace(_userMode) ? string.Empty : "+" + _userMode; }
        }

        public Dictionary<string, Channel> Channels {
            get { return _channels; }
        }

        // Events

        public event EventHandler<NickChangedEventArgs> NickChanged;
        public event EventHandler<QuitEventArgs> Quit;

        // Constructors

        public User( ) {
            _messageFilters = new List<FilterAndHandler> {
                new FilterAndHandler( ( m ) => ( m.Verb == "JOIN" ), HandleJoin ),
                new FilterAndHandler( ( m ) => ( m.Verb == "NICK" ), HandleNick ),
                new FilterAndHandler( ( m ) => ( m.Verb == "QUIT" ), HandleQuit ),
            };
        }

        // Methods

        public override string ToString( ) {
            return NickName;
        }

        //
        // Implementation
        //

        // Property-backing fields

        protected Server _server;
        protected ProtocolHandler _protocolHandler;
        protected NickUserHost _nickUserHost = new NickUserHost( );
        protected string _realName;
        protected string _realHostName;
        protected string _userMode = string.Empty;
        protected Dictionary<string, Channel> _channels = new Dictionary<string, Channel>( );

        // Fields

        protected readonly List<FilterAndHandler> _messageFilters;

        // Methods

        // Event handlers

        protected virtual void HandleJoin( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleJoin: sender type='{0}' args='{1}'", sender.GetType( ).Name, string.Join( "','", ev.Message.Args ) );
        }

        protected virtual void HandleNick( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleNick: oldNick='{0}' newNick='{1}'", _nickUserHost.NickName, ev.Message.Args[0] );
            NickName = ev.Message.Args[0];
        }

        protected virtual void HandleQuit( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleQuit: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
            OnQuit( _server.LookUpOrRegisterUser( ev.Message.Origin ), string.Join( " ", ev.Message.Args ) );
        }

        // Event dispatchers

        protected virtual void OnNickChanged( string oldNick, string newNick ) {
            var handler = NickChanged;
            if ( null != handler ) {
                handler( this, new NickChangedEventArgs( oldNick, newNick ) );
            }
        }

        protected virtual void OnQuit( User user, string reason ) {
            var handler = Quit;
            if ( null != handler ) {
                handler( this, new QuitEventArgs( user, reason ) );
            }
        }

        //
        // INotifyPropertyChanged
        //

        // Events

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = "" ) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

    }

}
