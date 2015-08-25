using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zive.Irc.Configuration;
using Zive.Irc.IdentService;

namespace Zive.Irc.WpfUi {

    public partial class MainWindow: Window {

        //========================================
        // INTERFACE
        //========================================

        //
        // Properties
        //

        public InputProcessor InputProcessor {
            get { return _inputProcessor; }
            set { _inputProcessor = value; }
        }

        public CommandProcessor CommandProcessor {
            get { return _commandProcessor; }
            set { _commandProcessor = value; }
        }

        //
        // Constructors
        //

        public MainWindow( ) {
            InitializeComponent( );

            foreach ( var network in _configuration.Networks ) {
                var networkWrapper = new NetworkWrapper( network );
                networkWrapper.ConnectFailed    += NetworkWrapper_ConnectFailed;
                networkWrapper.ConnectStarted   += NetworkWrapper_ConnectStarted;
                networkWrapper.ConnectSucceeded += NetworkWrapper_ConnectSucceeded;
                networkWrapper.ServerChange     += NetworkWrapper_ServerChange;
                _networks.Add( network.RefId, networkWrapper );
                foreach ( var serverWrapper in networkWrapper.Servers ) {
                    serverWrapper.ConnectFailed    += ServerWrapper_ConnectFailed;
                    serverWrapper.ConnectStarted   += ServerWrapper_ConnectStarted;
                    serverWrapper.ConnectSucceeded += ServerWrapper_ConnectSucceeded;
                    _servers.Add( serverWrapper.RefId, serverWrapper );
                }
            }

            RestoreWindowState( );

            _commandProcessor = new CommandProcessor( );
            _inputProcessor = new InputProcessor {
                CommandProcessor = _commandProcessor
            };
        }

        //========================================
        // IMPLEMENTATION
        //========================================

        //
        // Private fields
        //

        private InputProcessor _inputProcessor;
        private CommandProcessor _commandProcessor;

        private readonly ConfigurationRoot _configuration = App.Configuration;

        private readonly Dictionary<int, NetworkWrapper> _networks = new Dictionary<int, NetworkWrapper>( );
        private NetworkWrapper _currentNetwork;

        private readonly Dictionary<int, ServerWrapper> _servers = new Dictionary<int, ServerWrapper>( );
        private ServerWrapper _currentServer;

#pragma warning disable 649
        private readonly Collection<ChannelWrapper> _channels = new Collection<ChannelWrapper>( );
        private ChannelWrapper _currentChannel;
#pragma warning restore 649

        private readonly Collection<IIrcPanel> _panels = new Collection<IIrcPanel>( );
        private IIrcPanel _currentPanel;

        private IdentServer _identServer;
        private int _identServerUseCount = 0;
        

#if DEBUG
        private DebugPanel _debugPanel;
        private TabItem _debugTab;
#endif

        //
        // Protected methods
        //

        protected virtual bool AddPanel( string name, string header, string type, out TabItem newTabItem, out IIrcPanel newPanel ) {
            newPanel = IrcPanelFactory.Create( type );
            if ( null == newPanel ) {
                Debug.Print( "MainWindow.AddPanel: Couldn't construct new panel of type '{0}'", type );
                newTabItem = null;
                return false;
            }

            newTabItem = new TabItem {
                Name = name,
                Header = header,
                Content = newPanel,
            };
            TabControl.Items.Add( newTabItem );

            return true;
        }

        //
        // Private methods
        //

        private void SaveWindowState( ) {
            var mwc = _configuration.MainWindowConfiguration;
            mwc.SavedLeft = (int) Left;
            mwc.SavedTop = (int) Top;
            mwc.SavedWidth = (int) Width;
            mwc.SavedHeight = (int) Height;
            mwc.SavedState = (int) ( ( WindowState == WindowState.Minimized ) ? WindowState.Normal : WindowState );
        }

        private void RestoreWindowState( ) {
            var mwc = _configuration.MainWindowConfiguration;

            if ( mwc.SavedLeft > -1 && mwc.SavedTop > -1 ) {
                Left = mwc.SavedLeft;
                Top = mwc.SavedTop;
            }

            if ( mwc.SavedWidth > 0 && mwc.SavedHeight > 0 ) {
                Width = mwc.SavedWidth;
                Height = mwc.SavedHeight;
            }

            if ( -1 != mwc.SavedState ) {
                WindowState = (WindowState) mwc.SavedState;
            }
        }

        private void StartIdentServer( ) {
            if ( 1 == Interlocked.Increment( ref _identServerUseCount ) ) {
                _identServer = new IdentServer( );
                _identServer.Start( );
            }
        }

        private void StopIdentServer( ) {
            if ( 0 == Interlocked.Decrement( ref _identServerUseCount ) ) {
                _identServer.Stop( );
                _identServer = null;
            }
        }

        //
        // Event handlers
        //

        private void RoutedCommand_CanExecuteAlways( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = true;
        }

        private void RoutedCommand_GotFocus( object sender, RoutedEventArgs ev ) {
            _currentPanel.FocusInputBar( );
        }

        private void Window_Loaded( object sender, RoutedEventArgs ev ) {
            _panels.Add( ConsolePanel );

            // Configure console panel
            if ( _configuration.MainWindowConfiguration.ShowConsolePanelOnStartup ) {
                ConsoleTab.Visibility = Visibility.Visible;
                ConsolePanel.Visibility = Visibility.Visible;
                _currentPanel = ConsolePanel;
            }

#if DEBUG
            if ( _configuration.DebuggingConfiguration.ShowDebuggingOutputInConsoleTab ) {
                IIrcPanel panel;
                if ( AddPanel( "Debug", "Debug", "DebugPanel", out _debugTab, out panel ) ) {
                    _panels.Add( panel );
                    _debugPanel = (DebugPanel) panel;
                    _debugPanel.DebuggingOutputEnabled = true;
                }
            }
#endif

            ConsolePanel.InputProcessor = InputProcessor;
        }

        private void Window_Closing( object sender, CancelEventArgs ev ) {
            SaveWindowState( );
        }

        private void NetworkWrapper_ConnectStarted( object sender, EventArgs ev ) {
            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => ConsolePanel.ScrollbackManager.AddLine( ScrollbackParagraphMaker.Make(
                "Network ",
                FindResource( "NetworkNameStyle" ),
                _currentNetwork.Name,
                ": Attempting to connect to server ",
                FindResource( "ServerNameStyle" ),
                _currentServer.Name,
                "."
            ) ) ) );
        }

        private void NetworkWrapper_ConnectFailed( object sender, EventArgs ev ) {
            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => ConsolePanel.ScrollbackManager.AddLine( ScrollbackParagraphMaker.Make(
                "Network ",
                FindResource( "NetworkNameStyle" ),
                _currentNetwork.Name,
                ": Attempt to connect to server ",
                FindResource( "ServerNameStyle" ),
                _currentServer.Name,
                " failed."
            ) ) ) );
        }

        private void NetworkWrapper_ConnectSucceeded( object sender, EventArgs ev ) {
            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => ConsolePanel.ScrollbackManager.AddLine( ScrollbackParagraphMaker.Make(
                    "Network ",
                    FindResource( "NetworkNameStyle" ),
                    _currentNetwork.Name,
                    ": Connected to server ",
                    FindResource( "ServerNameStyle" ),
                    _currentServer.Name,
                    "."
            ) ) ) );
        }

        private void NetworkWrapper_ServerChange( object sender, ServerChangeEventArgs ev ) {
            _currentServer = ev.ServerWrapper;
        }

        private void ServerWrapper_ConnectStarted( object sender, EventArgs ev ) {
            StartIdentServer( );
        }

        private void ServerWrapper_ConnectFailed( object sender, EventArgs ev ) {
            StopIdentServer( );
        }

        private int _serverPanelSerial = 1;
        private void ServerWrapper_ConnectSucceeded( object sender, EventArgs ev ) {
            StopIdentServer( );

            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => {
                var serverWrapper = (ServerWrapper) sender;
                TabItem tab;
                IIrcPanel panel;

                if ( !AddPanel( "Server" + _serverPanelSerial++, _currentServer.ServerConfiguration.HostName, "ServerPanel", out tab, out panel ) ) {
                    return;
                }
                var serverPanel = (ServerPanel) panel;
                serverPanel.ServerWrapper = serverWrapper;
                serverPanel.NetworkWrapper = serverWrapper.NetworkWrapper;
                _panels.Add( panel );
                tab.Focus( );
                ConsolePanel.ScrollbackManager.AddLine( ScrollbackParagraphMaker.Make(
                    "Server ",
                    FindResource( "ServerNameStyle" ),
                    _currentServer.Name,
                    ": Connected."
                ) );
            } ) );
        }

        //
        // Command bindings: File menu
        //

        // FileExit

        private void FileExit_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            _debugPanel.DebuggingOutputEnabled = false;
            Close( );
        }

        //
        // Command bindings: View menu
        //

        //
        // Command bindings: Network menu
        //

        // NetworkConnect

        private void NetworkConnect_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            var dialog = new NetworkConnectDialog {
                Configuration = _configuration,
            };

            if ( !( dialog.ShowDialog( ) ?? false ) ) {
                Debug.Print( "MainWindow.NetworkConnect_Executed: cancelled" );
                return;
            }
            Debug.Print( "MainWindow.NetworkConnect_Executed: result is: Name='{0}' RefId='{1}'", dialog.Result.Name, dialog.Result.RefId );

            _currentNetwork = _networks[dialog.Result.RefId];
            _currentNetwork.Connect( );
        }

        // NetworkDisconnect

        private void NetworkDisconnect_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( null != _currentNetwork );
        }

        private void NetworkDisconnect_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            _currentNetwork.Disconnect( );
            _currentNetwork = null;
        }

        // NetworkManage

        private void NetworkManage_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        //
        // Command bindings: Server menu
        //

        // ServerConnect

        private void ServerConnect_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        // ServerDisconnect

        private void ServerDisconnect_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( null != _currentServer );
        }

        private void ServerDisconnect_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        // ServerManage

        private void ServerManage_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        //
        // Command bindings: Channel menu
        //

        // ChannelJoin

        private void ChannelJoin_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( null != _currentServer );
        }

        private void ChannelJoin_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        // ChannelPart

        private void ChannelPart_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( null != _currentChannel );
        }

        private void ChannelPart_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        // ChannelPartWithComment

        private void ChannelPartWithComment_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = ( null != _currentChannel );
        }

        private void ChannelPartWithComment_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        // ChannelManage

        private void ChannelManage_Executed( object sender, ExecutedRoutedEventArgs ev ) {

        }

        //
        // Command bindings: Help menu
        //

        // HelpAbout

        private void HelpAbout_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            new AboutDialog( ).ShowDialog( );
        }

    }

}
