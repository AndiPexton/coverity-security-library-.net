using System;
using Xunit;

namespace Coverity.Security.EscapeTests
{
    public class TestEscape
    {
        public static string[] WEB_NEW_LINES = {
        "\n", "\r", "\f",
        "\u2028", "\u2029"
        };

        public static string[] WEB_WHITESPACES = {
            " ", "\t"
        };

        public static string[] HTML_SENSITIVE_CHARS = {
            "<", ">",  // HTML tags
            "'", "\"", // HTML attributes
            " ", "/"   // HTML tag/attribute name
        };

        public static string[] JS_STRING_SENSITIVE_CHARS = {
            "'", "\"",     // JavaScript string transition
            "<", "/"       // Potential HTML </script> transition
        };

        public static string[] CSS_STRING_SENSITIVE_CHARS = {
            "'", "\"",     // CSS string transition
            "<", ">", "&"  // Potential HTML </style> transition
        };



        [Fact]
        public void TestHTMLEscaper_Transtions()
        {
            foreach (var sensitiveCharacter in HTML_SENSITIVE_CHARS)
            {
                Assert.False(Escape.Html(sensitiveCharacter).Contains(sensitiveCharacter));
            }
        }

        [Fact]
        public void TestCSSStringEscaper_Transtions()
        {
            foreach (var sensitiveCharacter in CSS_STRING_SENSITIVE_CHARS)
            {
                Assert.False(Escape.CssString(sensitiveCharacter).Contains(sensitiveCharacter));
            }
        }

        [Fact]
        public void TestJSStringEscaper_Transtions()
        {
            foreach (var sensitiveCharacter in JS_STRING_SENSITIVE_CHARS)
            {
                Assert.False(Escape.JsString(sensitiveCharacter).Contains(sensitiveCharacter));
            }
        }

        [Fact]
        public void TestAllStringEscaper_Transtions()
        {
            foreach (var sensitiveCharacter in WEB_NEW_LINES)
            {
                Assert.False(Escape.Html(sensitiveCharacter).Contains(sensitiveCharacter));
                Assert.False(Escape.CssString(sensitiveCharacter).Contains(sensitiveCharacter));
                Assert.False(Escape.JsString(sensitiveCharacter).Contains(sensitiveCharacter));
            }
        }

        [Fact]
        public void TestHtmlEscaper_Whitespace()
        {
            foreach (var sensitiveCharacter in WEB_WHITESPACES)
            {
                Assert.False(Escape.Html(sensitiveCharacter).Contains(sensitiveCharacter));
            }
        }

        [Fact]
        public void TestHTMLEscaper_String()
        {
            // Assume the string is within any HTML tag, like <div>:
            // <div>TAINTED_DATA_HERE</div>
            // or the content of an HTML attibute (not DOM event or CSS style)
            // <div data-param="TAINTED_DATA_HERE">...
            string beforeEscape = "</div><script src=\"http://example.com/?evil=true&param=xss\">"
                                + "\\ Foobar & '\"><img src=. onerorr=alert(1) > ";
            string afterEscape = Escape.Html(beforeEscape);

            string[] badSequences = {
            "<", ">", "<script", "</div", "\\", "'", " ", "& "
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestHTMLTextEscaper_String()
        {
            // This escaper Escape.htmlText is a relaxed version of the Escape.html
            // it only escapes ' " < > & and is sufficient when ALWAYS using quoted
            // attributes.
            //
            // Assume the string is within any HTML tag, like <div>:
            // <div>TAINTED_DATA_HERE</div>
            // or the content of an HTML attibute (not DOM event or CSS style)
            // <div data-param="TAINTED_DATA_HERE">...
            string beforeEscape = "</div><script src=\"http://example.com/?evil=true&param=xss\">"
                                + "Foobar & '\"><img src=. onerorr=alert(1) > ";
            string afterEscape = Escape.HtmlText(beforeEscape);

            string[] badSequences = {
            "<", ">", "<script", "</div",  "'", "\"", "& "
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestURIEncoder()
        {
            // Assume the string is within an HTML <script> tag, like so:
            // <a href="foobar?value=TAINTED_DATA_HERE">
            string beforeEscape = "close context'\" break context "
                                + "& + : % </script>"
                                + "\t \n \f \r (!#foobar$) *.*=?[@]";
            string afterEscape = Escape.Uri(beforeEscape);

            string[] badSequences = {
                "% ",
                "'", "\"",
                "+", "\t", "\n", "\f", "\r",
                "(", "!", "#", "$", ")", "*", ".", "=", "?",
                "[", "@", "]"
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestJSStringEscaper_String()
        {
            // Assume the string is within an HTML <script> tag, like so:
            // <script> var = 'TAINTED_DATA_HERE'; </script>
            string beforeEscape = "close context'\" continue context \\ break context "
                                + "\u2029 \u2028 escape HTML context & </script>"
                                + " control chars: \b \t \n \u000b \f %22";
            string afterEscape = Escape.JsString(beforeEscape);

            string[] badSequences = {
                "'",
                "\"",
                " \\ ",
                "\u2028",
                "\u2029",
                "&", "\b", "\t", "\n", "\u000b", "\f", "%",
                "</script>",
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestJSRegexEscaper_String()
        {
            // Assume the string is within a JavaScript regex:
            // <script> var b = /^TAINTED_DATA_HERE/.test("foo"); </script>
            string beforeEscape = "close context / continue context \\ break context "
                                + "\u2029 \u2028 escape HTML context & </script>"
                                + " ( ) [ ] { } * + - . ? ! ^ $ | "
                                + " control chars: \t \n \u000b \f \r ";
            string afterEscape = Escape.JsRegex(beforeEscape);

            string[] badSequences = {
                "\t", "\n", "\u000b", "\f", "\r",
                "</script>", " \\ ", " / ",
                " ( ", " ) ", " [ ", " ] ", " { ", " } ", " * ",
                " . ", " + ", " - ", " ? ", " ! ", " ^ ", " $ ",
                " | "
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestCSSStringEscaper_String()
        {
            // Assume the string is within an HTML <style> tag, like so:
            // <style> li [id *= 'TAINTED_DATA_HERE'] { ... } </style>
            string beforeEscape = "close context' \" continue context \\ break context \n"
                                + " escape HTML context </style>"
                                + " control chars: \b \t \n \f \r";
            string afterEscape = Escape.CssString(beforeEscape);

            string[] badSequences = {
                "'",
                "\\ ",
                "\n", "\r", "\t", "\f", "\r",
                "\"",
                "</style>",
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestNestedURIInHTMLEscaper_String()
        {
            // Assume the string is within an HTML <a> tag, like so:
            //   <a href="TAINTED_DATA_HERE">
            string beforeEscape = "javascript:alert(1); escape parent context \" "
                                + " break context % escape HTML context </a>"
                                + " data:text/html,<script>alert(1)</script>";
            string afterEscape = Escape.Html(Escape.Uri(beforeEscape));
            string[] badSequences = {
                "javascript:",
                "data:",
                "(1);",
                "\"",
                " % ",
                "</a>"
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestNestedURLInCSSInHTMLEscaper_String()
        {
            // Assume the string is within an HTML style attribute, like so:
            // <span style="background-image:url('TAINTED_DATA_HERE')">...
            string beforeEscape = "javascript:alert(1) break child context % close parent context ') escape"
                                + " parent context \" escape parent context </span>";
            string afterEscape = Escape.Html(Escape.CssString(Escape.Uri(beforeEscape)));
            string[] badSequences = {
                "javascript:",
                "javascript&#3A;", // shouldn't occur, in case it did would still fire javascript: uri
                " % ",
                " &#25; ",
                "')",
                "\n",
                "\"",
                "</span>"
            };

            foreach (var badSequence in badSequences)
            {
                Assert.False(afterEscape.Contains(badSequence));
            }
        }

        [Fact]
        public void TestForNullInput()
        {
            // The test for null inputs is useful to make sure that we do not throw an 
            // exception when receiving an null EL variable (quite common scenario) 
            try
            {
                Escape.Html(null);
                Escape.HtmlText(null);
                Escape.JsString(null);
                Escape.JsRegex(null);
                Escape.CssString(null);
                Escape.Uri(null);
                Escape.UriParam(null);
                Escape.SqlLikeClause(null, '\\');
                Escape.SqlLikeClause(null);
            }
            catch (Exception ex)
            {
                // Test must fail if any exception is thrown
                Assert.True(false);
            }
        }


        [Fact]
        public void TestSQLLikeEscaper_String()
        {
            Assert.True(Escape.SqlLikeClause("%_@'+=").Equals("@%@_@@'+="));
            Assert.True(Escape.SqlLikeClause("%_@'+=\\", '\\').Equals("\\%\\_@'+=\\\\"));
        }
    }
}
