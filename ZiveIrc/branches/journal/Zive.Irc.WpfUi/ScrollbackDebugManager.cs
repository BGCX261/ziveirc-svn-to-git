using System.Windows.Documents;
using System.Windows.Media;

namespace Zive.Irc.WpfUi {

    public class ScrollbackDebugManager: ScrollbackManager {

        protected override void _AddLineImpl( string text ) {
            FlowDocument.Blocks.Add( new Paragraph( new Run( text ) ) {
                Background = Brushes.MediumBlue,
                Foreground = Brushes.White,
            } );
        }

        protected override void _AddLineImpl( Inline inline ) {
            FlowDocument.Blocks.Add( new Paragraph( inline ) {
                Background = Brushes.MediumBlue,
                Foreground = Brushes.White,
            } );
        }

    }

}
