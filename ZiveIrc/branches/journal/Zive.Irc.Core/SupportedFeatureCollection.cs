using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zive.Irc.Core {

    public class SupportedFeatureCollection {

        public SupportedFeatureCollection( ) {
            BooleanFeatures = new Dictionary<string, bool>( );
            StringFeatures = new Dictionary<string, string>( );
            BooleanFeatureKeys = new ObservableCollection<string>( );
            StringFeatureKeys = new ObservableCollection<string>( );
        }

        public SupportedFeatureCollection( List<string> featureList ): this( ) {
            ParseFeatureList( featureList );
        }

        public void ParseFeatureList( List<string> featureList ) {
            for ( var n = 0; n < featureList.Count - 2; n++ ) {
                var arg = featureList[ n ];
                var equalsPos = arg.IndexOf( '=' );
                if ( -1 != equalsPos ) {
                    var feature = arg.Substring( 0, equalsPos );
                    var value = arg.Substring( equalsPos + 1 );
                    AddFeature( feature, value );
                } else {
                    AddFeature( arg );
                }
            }
        }

        public Dictionary<string, bool> BooleanFeatures { get; private set; }
        public Dictionary<string, string> StringFeatures { get; private set; }
        public ObservableCollection<string> BooleanFeatureKeys { get; private set; }
        public ObservableCollection<string> StringFeatureKeys { get; private set; }

        public void AddFeature( string feature ) {
            BooleanFeatures.Add( feature, true );
            BooleanFeatureKeys.Add( feature );
        }

        public void AddFeature( string feature, string value ) {
            StringFeatures.Add( feature, value );
            StringFeatureKeys.Add( feature );
        }

        public bool HasFeature( string feature ) {
            return BooleanFeatures.ContainsKey( feature ) || StringFeatures.ContainsKey( feature );
        }

        public bool FeatureHasValue( string feature ) {
            return StringFeatures.ContainsKey( feature );
        }

        public string GetFeatureValue( string feature ) {
            return StringFeatures[ feature ];
        }

    }

}
