using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Zive.Irc.WpfUi {

    public partial class MainWindow: Window {

        //========================================
        // INTERFACE
        //========================================

        //
        // Properties
        //

        public InputProcessor InputProcessor { get; set; }
        public CommandProcessor CommandProcessor { get; set; }

        //
        // Constructors
        //

        public MainWindow( ) {
            InitializeComponent( );

            _app = (App) Application.Current;
            _configuration = _app.ConfigurationManager.Configuration;
            foreach ( var network in _configuration.Networks ) {
                var networkWrapper = new NetworkWrapper( network );
                networkWrapper.ConnectFailed += NetworkWrapper_ConnectFailed;
                networkWrapper.ConnectStarted += NetworkWrapper_ConnectStarted;
                networkWrapper.ConnectSucceeded += NetworkWrapper_ConnectSucceeded;
                networkWrapper.ServerChange += NetworkWrapper_ServerChange;
                _networks.Add( network.RefId, networkWrapper );
                foreach ( var serverWrapper in networkWrapper.Servers ) {
                    _servers.Add( serverWrapper.RefId, serverWrapper );
                }
            }

            RestoreWindowState( );

            InputProcessor = new InputProcessor {
                CommandProcessor = ( CommandProcessor = new CommandProcessor( ) )
            };
        }

        // Commands

        //========================================
        // IMPLEMENTATION
        //========================================

        //
        // Private  fields
        //

        private readonly App _app;
        private readonly Configuration.Configuration _configuration;

        private readonly Dictionary<int,NetworkWrapper> _networks = new Dictionary<int, NetworkWrapper>( );
        private NetworkWrapper _currentNetwork;

        private readonly Dictionary<int,ServerWrapper> _servers = new Dictionary<int, ServerWrapper>( );
        private ServerWrapper _currentServer;

        private readonly Collection<ChannelWrapper> _channels = new Collection<ChannelWrapper>( );
        private ChannelWrapper _currentChannel;

        private readonly Collection<IIrcPanel> _panels = new Collection<IIrcPanel>( );
        private IIrcPanel _currentPanel;

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
            var left = (int) Left;
            var top = (int) Top;
            var width = (int) Width;
            var height = (int) Height;
            var state = (int) ( ( WindowState == WindowState.Minimized ) ? WindowState.Normal : WindowState );
            Debug.Print( "SaveWindowState: saving size, position, and state: ({0},{1}) {2}×{3} {4}", left, top, width, height, WindowState );

            var mwc = _configuration.MainWindowConfiguration;
            mwc.SavedLeft = left;
            mwc.SavedTop = top;
            mwc.SavedWidth = width;
            mwc.SavedHeight = height;
            mwc.SavedState = state;
        }

        private void RestoreWindowState( ) {
            var mwc = _configuration.MainWindowConfiguration;
            double left = mwc.SavedLeft;
            double top = mwc.SavedTop;
            double width = mwc.SavedWidth;
            double height = mwc.SavedHeight;
            var state = mwc.SavedState;
            Debug.Print( "RestoreWindowState: restoring size, position, and state: {0}×{1} ({2},{3}) {4}", width, height, left, top, ( -1 == state ? "Unsaved" : WindowState.ToString( ) ) );

            if ( left < 0.0d && top < 0.0d ) {
                Left = left;
                Top = top;
            }

            if ( width < 0.0d && height < 0.0d ) {
                Width = width;
                Height = height;
            }

            if ( -1 != state ) {
                WindowState = (WindowState) state;
            }
        }

        //
        // Event handlers
        //

        private void RoutedCommand_CanExecuteAlways( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = true;
        }

        private void RoutedCommand_GotFocus( object sender, RoutedEventArgs ev ) {
            _currentPanel.FocusTextInputArea( );
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
                    _debugPanel = (DebugPanel) panel;
                    _debugPanel.DebuggingOutputEnabled = true;
                    _debugPanel.InputProcessor = InputProcessor;
                }
            }
#endif

            ConsolePanel.InputProcessor = InputProcessor;
        }

        private void Window_Closing( object sender, CancelEventArgs ev ) {
            SaveWindowState( );
        }

        private void NetworkWrapper_ConnectStarted( object sender, EventArgs eventArgs ) {
            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => {
                var block = new Paragraph( );
                block.Inlines.Add( new Run( "Network " ) );
                block.Inlines.Add( new Run {
                    Text = _currentNetwork.Name,
                    Style = FindResource( "NetworkNameStyle" ) as Style
                } );
                block.Inlines.Add( new Run( ": Starting connection attempt to server " ) );
                block.Inlines.Add( new Run {
                    Text = _currentServer.Name,
                    Style = FindResource( "ServerNameStyle" ) as Style
                } );
                block.Inlines.Add( new Run( "." ) );
                ConsolePanel.ScrollbackManager.AddLine( block );
            } ) );
        }

        private void NetworkWrapper_ConnectFailed( object sender, EventArgs eventArgs ) {
            ConsolePanel.Dispatcher.BeginInvoke( (Action) ( ( ) => {
                var block = new Paragraph( );
                block.Inlines.Add( new Run( "Network " ) );
                block.Inlines.Add( new Run {
                    Text = _currentNetwork.Name,
                    Style = FindResource( "NetworkNameStyle" ) as Style
                } );
                block.Inlines.Add( new Run( ": Connection attempt to server " ) );
                block.Inlines.Add( new Run {
                    Text = _currentServer.Name,
                    Style = FindResource( "ServerNameStyle" ) as Style
                } );
                block.Inlines.Add( new Run( " failed." ) );
                ConsolePanel.ScrollbackManager.AddLine( block );
            } ) );
        }

        private void NetworkWrapper_ConnectSucceeded( object sender, EventArgs eventArgs ) {
            //throw new NotImplementedException( );
        }

        private void NetworkWrapper_ServerChange( object sender, ServerChangeEventArgs ev ) {
            _currentServer = ev.ServerWrapper;
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

            _currentNetwork = _networks[ dialog.Result.RefId ];
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
