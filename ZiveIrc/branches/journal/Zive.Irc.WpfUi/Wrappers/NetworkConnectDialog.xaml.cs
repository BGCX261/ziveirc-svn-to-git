using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Zive.Irc.Configuration;

namespace Zive.Irc.WpfUi {

    public partial class NetworkConnectDialog: Window {

        public ConfigurationRoot Configuration;

        public NetworkConfiguration Result;

        public NetworkConnectDialog( ) {
            InitializeComponent( );
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            NetworkListView.ItemsSource = Configuration.Networks;
            NetworkListView.Focus( );
        }

        private void NameColumn_SortAscendingClicked( object sender, RoutedEventArgs ev ) {
            _SetSortOrder( NetworkListView.ItemsSource, "Name", ListSortDirection.Ascending );
        }

        private void NameColumn_SortDescendingClicked( object sender, RoutedEventArgs ev ) {
            _SetSortOrder( NetworkListView.ItemsSource, "Name", ListSortDirection.Descending );
        }

        private void DescriptionColumn_SortAscendingClicked( object sender, RoutedEventArgs ev ) {
            _SetSortOrder( NetworkListView.ItemsSource, "Description", ListSortDirection.Ascending );
        }

        private void DescriptionColumn_SortDescendingClicked( object sender, RoutedEventArgs ev ) {
            _SetSortOrder( NetworkListView.ItemsSource, "Description", ListSortDirection.Descending );
        }

        private void GridViewRowPresenter_MouseLeftButtonDown( object sender, MouseButtonEventArgs ev ) {
            if ( 2 == ev.ClickCount ) {
                Close_Executed( this, null );
            }
        }

        private void Close_CanExecute( object sender, CanExecuteRoutedEventArgs ev ) {
            ev.CanExecute = true;
        }

        private void Close_Executed( object sender, ExecutedRoutedEventArgs ev ) {
            Result = NetworkListView.SelectedItem as NetworkConfiguration;
            DialogResult = ( Result != null );
            Close( );
        }

        private void _SetSortOrder( IEnumerable itemsSource, string propertyName, ListSortDirection direction ) {
            var view = CollectionViewSource.GetDefaultView( itemsSource );
            view.SortDescriptions.Clear( );
            view.SortDescriptions.Add( new SortDescription( propertyName, direction ) );
            view.Refresh( );
        }

    }

}
