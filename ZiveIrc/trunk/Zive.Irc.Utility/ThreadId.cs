using System.Threading;

namespace Zive.Irc.Utility {

    public static class ThreadId {

        public static int Get( ) {
            return Thread.CurrentThread.ManagedThreadId;
        }

    }

}
