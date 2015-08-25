using System.Windows.Input;

namespace Zive.Irc.WpfUi {

    public static class RoutedCommands {

        public static RoutedCommand NetworkConnect = new RoutedCommand( );
        public static RoutedCommand NetworkDisconnect = new RoutedCommand( );
        public static RoutedCommand NetworkManage = new RoutedCommand( );

        public static RoutedCommand ServerConnect = new RoutedCommand( );
        public static RoutedCommand ServerDisconnect = new RoutedCommand( );
        public static RoutedCommand ServerManage = new RoutedCommand( );

        public static RoutedCommand ChannelJoin = new RoutedCommand( );
        public static RoutedCommand ChannelPart = new RoutedCommand( );
        public static RoutedCommand ChannelPartWithComment = new RoutedCommand( );
        public static RoutedCommand ChannelManage = new RoutedCommand( );

        public static RoutedCommand HelpAbout = new RoutedCommand( );

        public static RoutedCommand LineInput = new RoutedCommand( );
        public static RoutedCommand ScrollAction = new RoutedCommand( );
        public static RoutedCommand HistoryAction = new RoutedCommand( );

    }

}
