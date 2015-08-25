using System;
using System.Globalization;

namespace Zive.Irc.Utility {

    public static class IntHelper {

        public static void TryParse( string str, Action<int> result ) {
            try {
                result( int.Parse( str, NumberStyles.Integer ) );
            }
            catch ( Exception ) {
            }
        }

    }

}