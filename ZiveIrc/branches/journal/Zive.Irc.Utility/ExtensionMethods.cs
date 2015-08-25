using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zive.Irc.Utility {

    public static class DictionaryExtensions {

        public static void AddIfMissing<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key, TValue value ) {
            if ( !dict.ContainsKey( key ) ) {
                dict.Add( key, value );
            }
        }

        public static void RemoveIfContains<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key ) {
            if ( dict.ContainsKey( key ) ) {
                dict.Remove( key );
            }
        }

        public static void ReplaceOrAdd<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key, TValue value ) {
            dict.RemoveIfContains( key );
            dict.Add( key, value );
        }

    }

    public static class ExceptionExtensions {

        public static int GetHResult( this Exception exception ) {
            return Marshal.GetHRForException( exception );
        }

    }

    public static class StringExtensions {

        private readonly static Dictionary< char, string > EscapeMap = new Dictionary<char, string> {
            { '\'', @"\'" },
            { '\"', @"\""" },
            { '\\', @"\\" },
            { '\0', @"\0" },
            { '\a', @"\a" },
            { '\b', @"\b" },
            { '\f', @"\f" },
            { '\n', @"\n" },
            { '\r', @"\r" },
            { '\t', @"\t" },
            { '\v', @"\v" },
        };

        public static string Escape( this string str ) {
            var result = string.Empty;

            foreach ( var ch in str ) {
                if ( EscapeMap.ContainsKey( ch ) ) {
                    result += EscapeMap[ ch ];
                } else if ( ch < 32 ) {
                    result += @"\c" + ( ch + 64 );
                } else {
                    result += ch;
                }
            }

            return result;
        }

    }

}
