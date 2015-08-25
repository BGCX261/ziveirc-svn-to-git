using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zive.Irc.Utility {

    public static class ListHelper {

        public static IList<KeyValuePair<string, string>> ParseList( string str ) {
            var result = new Collection<KeyValuePair<string, string>>( );
            foreach ( var segment in str.Split( CommonDelimiters.Comma, StringSplitOptions.None ) ) {
                try {
                    var index = segment.IndexOf( ':' );
                    result.Add( new KeyValuePair<string, string>( segment.Substring( 0, index ), segment.Substring( index + 1 ) ) );
                }
                catch ( Exception ) { }
            }
            return result;
        }

        public static IList<KeyValuePair<char, TValue>> ExplodeKeys<TValue>( IList<KeyValuePair<string, TValue>> list ) {
            var result = new Collection<KeyValuePair<char, TValue>>( );
            foreach ( var pair in list ) {
                foreach ( char ch in pair.Key ) {
                    result.Add( new KeyValuePair<char, TValue>( ch, pair.Value ) );
                }
            }
            return result;
        }

        public static IList<KeyValuePair<TKey, int>> ParseValuesAsInt<TKey>( IList<KeyValuePair<TKey, string>> list ) {
            var result = new Collection<KeyValuePair<TKey, int>>( );
            foreach ( var pair in list ) {
                var key = pair.Key;
                IntHelper.TryParse( pair.Value, value => result.Add( new KeyValuePair<TKey, int>( key, value ) ) );
            }
            return result;
        }

        public static void AddListToDictionary<TKey, TValue>( IList<KeyValuePair<TKey, TValue>> list, Dictionary<TKey, TValue> dict ) {
            foreach ( var pair in list ) {
                dict.ReplaceOrAdd( pair.Key, pair.Value );
            }
        }

    }

}
