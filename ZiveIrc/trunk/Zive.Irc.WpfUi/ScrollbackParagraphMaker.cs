using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Zive.Irc.WpfUi {

    public static class ScrollbackParagraphMaker {

        public static Paragraph Make( params object[ ] args ) {
            var paragraph = new Paragraph( );
            Style style = null;
            FontStyle fontStyle = FontStyles.Normal;
            bool hasFontStyle = false;
            Run run = null;

            foreach ( var arg in args ) {
                if ( arg is Style ) {
                    style = (Style) arg;
                    continue;
                }
                if ( arg is FontStyle ) {
                    fontStyle = (FontStyle) arg;
                    hasFontStyle = true;
                    continue;
                }

                if ( arg is string ) {
                    run = new Run( (string) arg );
                } else if ( arg is Run ) {
                    run = (Run) arg;
                }
                if ( null == run ) {
                    continue;
                }

                if ( null != style ) {
                    run.Style = style;
                    style = null;
                }
                if ( hasFontStyle ) {
                    run.FontStyle = fontStyle;
                    fontStyle = FontStyles.Normal;
                    hasFontStyle = false;
                }

                paragraph.Inlines.Add( run );
                run = null;
            }

            return paragraph;
        }

    }

}
