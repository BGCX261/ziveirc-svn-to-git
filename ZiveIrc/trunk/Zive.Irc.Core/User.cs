using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Zive.Irc.Core.Annotations;

namespace Zive.Irc.Core {

    public class User: INotifyPropertyChanged, ITarget {

        //
        // Interface
        //

        // Properties

        public virtual ProtocolHandler ProtocolHandler {
            get { return _protocolHandler; }
            set {
                if ( null != _protocolHandler ) {
                    UnregisterVerbs( );
                }
                _protocolHandler = value;
                if ( null != _protocolHandler ) {
                    RegisterVerbs( );
                }
                OnPropertyChanged( "ProtocolHandler" );
            }
        }

        public virtual Server Server {
            get { return _server; }
            set {
                if ( null != _server ) {
                    UnregisterServerHandlers( );
                }
                _server = value;
                if ( null != _server ) {
                    RegisterServerHandlers( );
                }
                OnPropertyChanged( "Server" );
            }
        }

        public virtual string NickName {
            get { return _nickUserHost.NickName; }
            set {
                string oldNick = _nickUserHost.NickName;
                _nickUserHost.NickName = value;
                OnNickChanged( oldNick, value );
                OnPropertyChanged( "NickName" );
            }
        }

        public virtual string UserName {
            get { return _nickUserHost.UserName; }
            set {
                _nickUserHost.UserName = value;
                OnPropertyChanged( "UserName" );
            }
        }

        public virtual string HostName {
            get { return _nickUserHost.HostName; }
            set {
                _nickUserHost.HostName = value;
                OnPropertyChanged( "HostName" );
            }
        }

        public virtual string RealName { get; set; }
        public virtual string RealHostName { get; set; }

        public virtual string UserHost { get { return _nickUserHost.UserHost; } }
        public virtual NickUserHost NickUserHost { get { return _nickUserHost; } set { _nickUserHost = value; } }

        public string UserMode { get { return "+" + _userMode; } }

        public Dictionary<string, Channel> Channels { get; private set; }

        // Events

        public event EventHandler<NickChangedEventArgs> NickChanged;

        // Constructors

        public User( ) {
            Channels = new Dictionary<string, Channel>( );

            _targetedEvents = new List<TargetedEvent> {
                new TargetedEvent { EventKey = "JOIN",      Handler = HandleJoin     },
                new TargetedEvent { EventKey = "NOTICE",    Handler = HandleNotice   },
                new TargetedEvent { EventKey = "PRIVNSG",   Handler = HandlePrivMsg  },
                new TargetedEvent { EventKey = "QUIT",      Handler = HandleQuit     },
                new TargetedEvent { EventKey = "MODE:user", Handler = HandleUserMode },
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

        private NickUserHost _nickUserHost = new NickUserHost( );
        protected ProtocolHandler _protocolHandler;
        protected Server _server;
        private string _userMode = string.Empty;

        // Fields

        protected Dictionary<string, EventHandler<MessageEventArgs>> VerbsToRegister = new Dictionary<string, EventHandler<MessageEventArgs>>( );

        // Methods

        protected virtual void RegisterVerbs( ) {
            foreach ( var item in VerbsToRegister ) {
                ProtocolHandler.MessageDiscriminator[ item.Key ] += item.Value;
            }
        }

        protected virtual void UnregisterVerbs( ) {
            foreach ( var item in VerbsToRegister ) {
                ProtocolHandler.MessageDiscriminator[ item.Key ] -= item.Value;
            }
        }

        protected List<TargetedEvent> _targetedEvents;

        protected virtual void RegisterServerHandlers( ) {
            _server.RegisterUser( _nickUserHost.NickName, this );
            _server.EventManager.BulkSubscribe( _targetedEvents );
        }

        protected virtual void UnregisterServerHandlers( ) {
            _server.EventManager.BulkUnsubscribe( _targetedEvents );
            _server.UnregisterUser( _nickUserHost.NickName );
        }

        // Event handlers

        protected virtual void HandleJoin( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleJoin: typeof(sender)={0} args={1}", sender.GetType( ), string.Join( "','", ev.Message.Args ) );
        }

        protected virtual void HandleNotice( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleNotice: args='{0}'", string.Join( "','", ev.Message.Args ) );
        }

        protected virtual void HandlePrivMsg( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandlePrivMsg: origin='{0}' target='{1}' args='{2}'", ev.Message.Origin, ev.Message.Target, string.Join( "','", ev.Message.Args ) );
        }

        protected virtual void HandleQuit( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleQuit: args={0}", string.Join( "','", ev.Message.Args ) );
        }

        protected virtual void HandleUserMode( object sender, MessageEventArgs ev ) {
            Debug.Print( "User.HandleUserMode: target='{0}' mode change='{1}' mode args={2}", ev.Message.Target, ev.Message.Args[ 0 ], ev.Message.Args.Count > 1 ? "'" + ev.Message.Args[ 1 ] + "'" : "(none)" );

            var index = _userMode.IndexOf( ' ' );
            var currentMode = ( index > -1 ) ? _userMode.Substring( 0, index ) : _userMode;
            var positive = true;
            foreach ( var ch in ev.Message.Args[ 0 ] ) {
                switch ( ch ) {
                    case '+':
                        positive = true;
                        break;

                    case '-':
                        positive = false;
                        break;

                    default:
                        index = currentMode.IndexOf( ch );
                        if ( index > -1 ) {
                            // already in currentModes
                            if ( !positive ) {
                                currentMode = currentMode.Remove( index, 1 );
                            }
                        } else {
                            // not in currentModes
                            if ( positive ) {
                                currentMode += ch;
                            }
                        }
                        break;
                }
            }
            _userMode = currentMode;

            Debug.Print( "User.HandleUserMode: new user mode: {0}", _userMode );
        }

        // Event dispatchers

        protected virtual void OnNickChanged( string oldNick, string newNick ) {
            var handler = NickChanged;
            if ( null != handler ) {
                handler( this, new NickChangedEventArgs( oldNick, newNick ) );
            }
        }

        //
        // INotifyPropertyChanged
        //

        // Events

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( string propertyName ) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

    }

}
