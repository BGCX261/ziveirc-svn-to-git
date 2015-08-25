using System;
using System.Windows.Documents;

namespace Zive.Irc.WpfUi {

    public class ScrollbackManager {

        public FlowDocument FlowDocument { get; set; }

        public void AddLine( string format, params object[ ] args ) {
            FlowDocument.Dispatcher.BeginInvoke( (Action<string>) _AddLineImpl, string.Format( format, args ) );
        }

        public void AddLine( Inline inline ) {
            FlowDocument.Dispatcher.BeginInvoke( (Action<Inline>) _AddLineImpl, inline );
        }

        public void AddLine( Block block ) {
            FlowDocument.Dispatcher.BeginInvoke( (Action<Block>) _AddLineImpl, block );
        }

        protected virtual void _AddLineImpl( string text ) {
            FlowDocument.Blocks.Add( new Paragraph( new Run( text ) ) );
        }

        protected virtual void _AddLineImpl( Inline inline ) {
            FlowDocument.Blocks.Add( new Paragraph( inline ) );
        }

        protected virtual void _AddLineImpl( Block block ) {
            FlowDocument.Blocks.Add( block );
        }

    }

}
