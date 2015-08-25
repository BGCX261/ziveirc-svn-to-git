using System.Collections.Generic;
using Zive.Irc.Utility;

namespace Zive.Irc.Configuration {

    internal static class RefIdManager {

        private static readonly object _lock = new object( );
        private static int _nextId = 1;
        private static readonly Dictionary<int,object> _registeredObjects = new Dictionary<int, object>( );

        public static int GetNext( ) {
            lock ( _lock ) {
                return _nextId++;
            }
        }

        public static object GetObject( int id ) {
            lock ( _lock ) {
                if ( _registeredObjects.ContainsKey( id ) ) {
                    return _registeredObjects[ id ];
                }
            }
            return null;
        }

        public static int Peek( ) {
            lock ( _lock ) {
                return _nextId;
            }
        }

        public static void Register( int id, object o ) {
            lock ( _lock ) {
                _registeredObjects.Add( id, o );
            }
        }

        public static void Reset( ) {
            lock ( _lock ) {
                _nextId = 1;
                _registeredObjects.Clear( );
            }
        }

        public static void Set( int nextId ) {
            lock ( _lock ) {
                _nextId = nextId;
            }
        }

        public static void Unregister( int id ) {
            lock ( _lock ) {
                _registeredObjects.RemoveIfContains( id );
            }
        }

    }

}
