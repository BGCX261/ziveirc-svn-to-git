namespace Zive.Irc.Core {

    public interface IServerComponent {

        Server Server { get; set; }

        void RegisterMessages( );
        void UnregisterMessages( );

    }

}
