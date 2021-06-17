using System;
using Xunit;
using Coverity.Security;

namespace Coverity.Security.FilterTests
{
    public class TestFilter
    {
        string[] colorTrueTests = {
                //Named Color
                "AliceBlue",
                "white",
                "PaleVioletRed",

                //Hex Color
                "#fff",
                "#FFF",
                "#0fF056"
        };

        string[] colorFalseTests = {
                //named Color
                "#1",
                "this is not a name",
                "efe fef",
                "foo()<>{}",
                "\09 thisIsPossibleButNotConsidered",

                //Hex Color
                "#1",
                "12345",
                "#12",
                "#1223",
                "#12233",
                "#122g34",
                "\0#123",
                "\f#123",
                "\n#123",
                ""
        };

        [Fact]
        public void TestAsCssColorDefault_Invalid()
        {
            string defaultColour = "blue";
            foreach (var color in colorFalseTests)
            {
                
                string filtered = Filter.AsCssColor(color, defaultColour);
                Assert.True(filtered == defaultColour);

            }
        }

        [Fact]
        public void TestAsCssColor_Invalid()
        {
            string invalid = "invalid";
            foreach (var color in colorFalseTests)
            {
                string filtered = Filter.AsCssColor(color);
                Assert.True(filtered == invalid);

            }
        }

        [Fact]
        public void TestAsCssColorDefault_Valid()
        {
            string defaultColour = "blue";
            foreach (var color in colorTrueTests)
            {
                string filtered = Filter.AsCssColor(color, defaultColour);
                Assert.True(filtered == color);

            }
        }

        [Fact]
        public void TestAsCssColor_Valid()
        {
            foreach (var color in colorTrueTests)
            {
                string filtered = Filter.AsCssColor(color);
                Assert.True(filtered == color);
            }
        }


        string[] numberFalseTests = {
            //asNumber
            ".",
            "+65266+",
            "-+1.266",
            "65.65.",

            //asHex
            "0xefefefg",
            "0xag",
            "abc",
            "\\x15"
        };
        string[] numberTrueTests = {
            //asNumber
            "+1.425",
            "65.",
            "-64.32",
            "42",
            "-.04",
            "0.2323232",

            //asHex
            "0xefefef",
            "0x0ff",
            "0x234345"
        };

        [Fact]
        public void TestAsNumber_Valid()
        {
            foreach (var number in numberTrueTests)
            {
                string filtered = Filter.AsNumber(number);
                Assert.True(filtered == number);
            }
        }

        [Fact]
        public void TestAsNumberDefault_Valid()
        {
            string defaultNumber = "1";
            foreach (var number in numberTrueTests)
            {
                string filtered = Filter.AsNumber(number, defaultNumber);
                Assert.True(filtered == number);
            }
        }

        [Fact]
        public void TestAsNumberDefault_Invalid()
        {
            string defaultNumber = "1";
            foreach (var number in numberFalseTests)
            {
                string filtered = Filter.AsNumber(number, defaultNumber);
                Assert.True(filtered == defaultNumber);
            }
        }

        [Fact]
        public void TestAsNumber_Invalid()
        {
            string defaultNumber = "0";
            foreach (var number in numberFalseTests)
            {
                string filtered = Filter.AsNumber(number);
                Assert.True(filtered == defaultNumber);
            }
        }


        [Fact]
        public void TestAsNumberOctal_Valid()
        {
            string octal = "0777";
            string filtered = Filter.AsNumber(octal);
            Assert.True(Convert.ToInt32(filtered) == Convert.ToInt32(octal));
        }

        string[] urlFalseTests = {
            "javascript:test('http:')",
            "jaVascRipt:test",
            "\\UNC-PATH\\",
            "data:test",
            "about:blank",
            "javascript\n:",
            "vbscript:IE",
            "data&#58boo",
            "dat\0a:boo"
        };

        string[] urlTrueTests = {
            "\\\\UNC-PATH\\",
            "http://host/url",
            "hTTp://host/url",
            "//coverity.com/lo",
            "/base/path",
            "https://coverity.com",
            "mailto:srl@coverity.com",
            "maiLto:srl@coverity.com",
            "ftp://coverity.com/elite.warez.tgz",
            ""
        };

        string[] urlFlexibleTrueTests = {
                "tel:5556667777",
                "gopher:something something",
                "test.html"
        };

        [Fact]
        public void TestFlexibleUrl_Valid()
        {
            foreach (var url in urlTrueTests)
            {
                string filtered = Filter.AsFlexibleURL(url);
                Assert.True(filtered == url);
            }
            foreach (var url in urlFlexibleTrueTests)
            {
                string filtered = Filter.AsFlexibleURL(url);
                Assert.True(filtered == url);
            }
        }

        [Fact]
        public void TestFlexibleUrl_Invalid()
        {
            foreach (var url in urlFalseTests)
            {
                string filtered = Filter.AsFlexibleURL(url);
                Assert.True(filtered == "./" + url);
            }
        }

        [Fact]
        public void TestUrl_Invalid()
        {
            foreach (var url in urlFalseTests)
            {
                string filtered = Filter.AsURL(url);
                Assert.True(filtered == "./" + url);
            }
        }

        [Fact]
        public void TestUrl_Valid()
        {
            foreach (var url in urlTrueTests)
            {
                string filtered = Filter.AsURL(url);
                Assert.True(filtered == url);
            }
        }
    }
}
