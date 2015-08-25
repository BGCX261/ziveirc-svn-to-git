using System;
using System.Diagnostics;
using System.IO;

namespace Zive.Irc.Utility {

    public class FancyBuffer {

        public FancyBuffer( int size ) {
            Size = size;
            Buffer = new byte[ Size ];
        }

        public int Size;
        public int Offset;
        public byte[] Buffer;

        public int Available { get { return Size - Offset; } }
        public bool IsFull { get { return ( Available <= 0 ); } }

        public IAsyncResult BeginRead( Stream stream, AsyncCallback callback ) {
            if ( IsFull ) {
                Debug.Print( "FancyBuffer.BeginRead: buffer is full" );
                throw new Exception( "Read buffer is full." );
            }
            _stream = stream;
            return _stream.BeginRead( Buffer, Offset, Available, callback, this );
        }

        public int EndRead( IAsyncResult ar ) {
            return _stream.EndRead( ar );
        }

        public void Clear( ) {
            Offset = 0;
        }

        private Stream _stream;

    }

}
