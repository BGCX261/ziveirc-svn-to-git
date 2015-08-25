using System;
using System.Collections.ObjectModel;
using Zive.Irc.Utility;

namespace Zive.Irc.Core {

    public class ConnectionEstablishedEventArgs: EventArgs {

        public ConnectionEstablishedEventArgs( Server server ) {
            Server = server;
        }

        public Server Server { get; set; }

    }

    public class ConnectionAttemptStartedEventArgs: EventArgs {

        public ConnectionAttemptStartedEventArgs( SslEndPoint sslEndPoint ) {
            SslEndPoint = sslEndPoint;
        }

        public SslEndPoint SslEndPoint { get; set; }

    }

    public class ConnectionAttemptFailedEventArgs: EventArgs {

        public ConnectionAttemptFailedEventArgs( SslEndPoint sslEndPoint, Exception e ) {
            SslEndPoint = sslEndPoint;
            Exception = e;
        }

        public SslEndPoint SslEndPoint { get; set; }
        public Exception Exception { get; set; }

    }

    public class ConnectionFailedEventArgs: EventArgs {

        public ConnectionFailedEventArgs( Exception e ) {
            Exception = e;
        }

        public Exception Exception { get; set; }

    }

    public class MessageEventArgs: EventArgs {

        public MessageEventArgs( Message message ) {
            Message = message;
        }

        public Message Message { get; set; }

    }

    public class MotdCompleteEventArgs: EventArgs {

        public MotdCompleteEventArgs( Collection<string> lines ) {
            Lines = lines;
        }

        public Collection<string> Lines { get; set; }

    }

    public class NamesCompleteEventArgs: EventArgs {

        public NamesCompleteEventArgs( Collection<string> nickNames ) {
            NickNames = nickNames;
        }

        public Collection<string> NickNames { get; set; }

    }

    public class NickChangedEventArgs: EventArgs {

        public NickChangedEventArgs( string oldNick, string newNick ) {
            OldNick = oldNick;
            NewNick = newNick;
        }

        public string OldNick { get; set; }
        public string NewNick { get; set; }

    }

    public class NoticeReceivedEventArgs: EventArgs {

        public NoticeReceivedEventArgs( User origin, ITargetable target, string message ) {
            Origin = origin;
            Target = target;
            Message = message;
        }

        public User Origin { get; set; }
        public ITargetable Target { get; set; }
        public string Message { get; set; }

    }

    public class PrivMsgReceivedEventArgs: EventArgs {

        public PrivMsgReceivedEventArgs( User origin, ITargetable target, string message ) {
            Origin = origin;
            Target = target;
            Message = message;
        }

        public User Origin { get; set; }
        public ITargetable Target { get; set; }
        public string Message { get; set; }

    }

    public class QuitEventArgs: EventArgs {

        public QuitEventArgs( User user, string quitMessage ) {
            User = user;
            QuitMessage = quitMessage;
        }

        public User User { get; set; }
        public string QuitMessage { get; set; }

    }

    public class RegisteredEventArgs: EventArgs {

        public RegisteredEventArgs( string nickName ) {
            NickName = nickName;
        }

        public string NickName { get; set; }

    }

    public class ServerNoticeEventArgs: EventArgs {

        public ServerNoticeEventArgs( NickUserHost origin, string text ) {
            Origin = origin;
            Text = text;
        }

        public NickUserHost Origin { get; set; }
        public string Text { get; set; }

    }

    public class WhoCompleteEventArgs: EventArgs {

        public WhoCompleteEventArgs( object target, Collection<WhoResponse> responses ) {
            Target = target;
            Responses = responses;
        }

        public object Target { get; set; }
        public Collection<WhoResponse> Responses { get; set; }

    }

    public class WhoisCompleteEventArgs: EventArgs {

        public WhoisCompleteEventArgs( NickUserHost target, Collection<Message> responses ) {
            Target = target;
            Responses = responses;
        }

        public NickUserHost Target { get; set; }
        public Collection<Message> Responses { get; set; }

    }

}
