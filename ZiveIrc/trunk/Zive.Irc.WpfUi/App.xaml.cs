﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Zive.Irc.Configuration;

namespace Zive.Irc.WpfUi {

    public partial class App: Application {

        //
        // Public properties
        //

        public ConfigurationManager ConfigurationManager { get; set; }
        public static new App Current { get { return (App) ( Application.Current ); } }
        public static ConfigurationRoot Configuration { get { return Current.ConfigurationManager.Configuration; } }

        //
        // Private fields
        //

        private string _configurationPathName = string.Empty;
        private string _configurationFileName = string.Empty;

        //
        // Event handlers
        //

        private void Application_Startup( object sender, StartupEventArgs ev ) {
            Debug.Print( "Application_Startup:" );

            try {
                _configurationPathName = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ) + @"\Zive Technology Research\ZiveIRC";
                _configurationFileName = _configurationPathName + @"\configuration.xml";
            }
            catch ( Exception e ) {
                Debug.Print( "App.Application_Startup: Caught exception trying to obtain path of configuration folder:\n{0}", e );
                return;
            }

            try {
                if ( !string.IsNullOrWhiteSpace( _configurationPathName ) && !Directory.Exists( _configurationPathName ) ) {
                    Directory.CreateDirectory( _configurationPathName );
                }
            }
            catch ( Exception e ) {
                Debug.Print( "App.Application_Startup: Caught exception trying to create configuration folder '{0}':\n{1}", _configurationPathName, e );
            }

            Debug.Print( "App.Application_Startup: Creating ConfigurationManager object." );
            ConfigurationManager = new ConfigurationManager( );

            try {
                ConfigurationManager.Load( _configurationFileName );
                return;
            }
            catch ( Exception e ) {
                Debug.Print( "App.Application_Startup: Caught exception trying to load configuration from file '{0}':\n{1}", _configurationFileName, e );
            }

            try {
                Debug.Print( "App.Application_Startup: Making up configuration." );
                ConfigurationManager.Configuration = MakeUpConfiguration( );

                Debug.Print( "App.Application_Startup: Saving new made-up configuration." );
                ConfigurationManager.Save( _configurationFileName );
            }
            catch ( Exception e ) {
                Debug.Print( "App.Application_Startup: Caught exception trying to save default configuration to file '{0}':\n{1}", _configurationFileName, e );
            }
        }

        private void Application_Exit( object sender, ExitEventArgs ev ) {
            try {
                Debug.Print( "App.Application_Exit: Saving configuration." );
                ConfigurationManager.Save( _configurationFileName );
            }
            catch ( Exception e ) {
                Debug.Print( "App.Application_Exit: Caught exception trying to save configuration to file '{0}':\n{1}", _configurationFileName, e );
            }
        }

        private void Application_SessionEnding( object sender, SessionEndingCancelEventArgs ev ) {
            Debug.Print( "App.Application_SessionEnding: ReasonSessionEnding is {0}", ev.ReasonSessionEnding );
        }

        //
        // Private methods
        //

        private ConfigurationRoot MakeUpConfiguration( ) {
            var conf = new ConfigurationRoot( );

            var foonetic = new NetworkConfiguration {
                Name = "Foonetic",
                Description = "The IRC network where #xkcd can be found. ;3"
            };
            var freenode = new NetworkConfiguration {
                Name = "Freenode",
                Description = "lilo's network. I still miss him. =("
            };
            var testnet = new NetworkConfiguration {
                Name = "TestNet",
                Description = "My local testing network.",
            };
            var undernet = new NetworkConfiguration {
                Name = "Undernet",
                Description = "My very first IRC network, I started hanging out there before there were 500 simultaneous users network-wide."
            };

            var belayFooneticNet = new ServerConfiguration {
                Name = "Foonetic: US, NJ, Newark: belay.foonetic.net",
                Description = "zigdon's IRC server, hosted in Newark, NJ.",
                HostName = "belay.foonetic.net",
                Ports = new Collection<int> {
                    -7001, -6697, 7000, 6669, 6668, 6667,
                },
            };
            var ircFreenodeNet = new ServerConfiguration {
                Name = "Freenode: Round-robin: irc.freenode.net",
                Description = "Round robin hostname for Freenode.",
                HostName = "irc.freenode.net",
                Ports = new Collection<int> {
                    -6697, 6667,
                },
            };
            var ircUndernetOrg = new ServerConfiguration {
                Name = "Undernet: Round-robin: irc.undernet.org",
                Description = "Round robin hostname for Undernet.",
                HostName = "irc.undernet.org",
                Ports = new Collection<int> {
                    6667,
                },
            };
            var violetZiveCa = new ServerConfiguration {
                Name = "Zive: Charybdis/Atheme test: violet.zive.ca",
                Description = "Test Charybdis/Atheme server+services on violet.zive.ca.",
                HostName = "violet.zive.ca",
                Ports = new Collection<int> {
                    -6698, 6668
                },
            };

            foonetic.Servers.Add( belayFooneticNet );
            belayFooneticNet.Network = foonetic;

            freenode.Servers.Add( ircFreenodeNet );
            ircFreenodeNet.Network = freenode;

            testnet.Servers.Add( violetZiveCa );
            violetZiveCa.Network = testnet;

            undernet.Servers.Add( ircUndernetOrg );
            ircUndernetOrg.Network = undernet;

            conf.Networks.Add( foonetic );
            conf.Networks.Add( freenode );
            conf.Networks.Add( testnet );
            conf.Networks.Add( undernet );

            conf.Servers.Add( belayFooneticNet );
            conf.Servers.Add( ircFreenodeNet );
            conf.Servers.Add( ircUndernetOrg );
            conf.Servers.Add( violetZiveCa );

            conf.UserConfiguration = new UserConfiguration {
                NickName = "ZiveIrcTest",
                UserName = "IceKarma",
                RealName = "Testing instance of ZiveIrc. Contact: IceKarma",
            };

#if DEBUG
            conf.DebuggingConfiguration.ShowDebuggingOutputInConsoleTab = true;
#endif

            return conf;
        }

    }

}
