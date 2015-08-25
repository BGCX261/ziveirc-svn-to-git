using System;

namespace Zive.Irc.Core {

    public class FilterAndHandler {

        public FilterAndHandler( Func<Message, bool> filter, EventHandler<MessageEventArgs> handler ) {
            Filter = filter;
            Handler = handler;
        }

        public Func<Message, bool> Filter;
        public EventHandler<MessageEventArgs> Handler;

    }

}
