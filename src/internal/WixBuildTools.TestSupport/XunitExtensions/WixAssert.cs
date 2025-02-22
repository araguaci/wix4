// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixBuildTools.TestSupport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using WixBuildTools.TestSupport.XunitExtensions;
    using Xunit;

    public class WixAssert : Assert
    {
        public static void CompareLineByLine(string[] expectedLines, string[] actualLines)
        {
            var lineNumber = 0;

            for (; lineNumber < expectedLines.Length && lineNumber < actualLines.Length; ++lineNumber)
            {
                WixAssert.StringEqual($"{lineNumber}: {expectedLines[lineNumber]}", $"{lineNumber}: {actualLines[lineNumber]}");
            }

            var additionalExpectedLines = expectedLines.Length > lineNumber ? String.Join(Environment.NewLine, expectedLines.Skip(lineNumber).Select((s, i) => $"{lineNumber + i}: {s}")) : $"Missing {actualLines.Length - lineNumber} lines";
            var additionalActualLines = actualLines.Length > lineNumber ? String.Join(Environment.NewLine, actualLines.Skip(lineNumber).Select((s, i) => $"{lineNumber + i}: {s}")) : $"Missing {expectedLines.Length - lineNumber} lines";

            WixAssert.StringEqual(additionalExpectedLines, additionalActualLines);
        }

        public static void CompareXml(XContainer xExpected, XContainer xActual)
        {
            var expecteds = xExpected.Descendants().Select(x => $"{x.Name.LocalName}:{String.Join(",", x.Attributes().OrderBy(a => a.Name.LocalName).Select(a => $"{a.Name.LocalName}={a.Value}"))}");
            var actuals = xActual.Descendants().Select(x => $"{x.Name.LocalName}:{String.Join(",", x.Attributes().OrderBy(a => a.Name.LocalName).Select(a => $"{a.Name.LocalName}={a.Value}"))}");

            CompareLineByLine(expecteds.OrderBy(s => s).ToArray(), actuals.OrderBy(s => s).ToArray());
        }

        public static void CompareXml(string expectedPath, string actualPath)
        {
            var expectedDoc = XDocument.Load(expectedPath, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            var actualDoc = XDocument.Load(actualPath, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);

            CompareXml(expectedDoc, actualDoc);
        }

        /// <summary>
        /// Dynamically skips the test.
        /// Requires that the test was marked with a fact attribute derived from <see cref="WixBuildTools.TestSupport.XunitExtensions.SkippableFactAttribute" />
        /// or <see cref="WixBuildTools.TestSupport.XunitExtensions.SkippableTheoryAttribute" />
        /// </summary>
        public static void Skip(string message)
        {
            throw new SkipTestException(message);
        }

        public static void Succeeded(int hr, string format, params object[] formatArgs)
        {
            if (0 > hr)
            {
                throw new SucceededException(hr, String.Format(format, formatArgs));
            }
        }

        public static void StringCollectionEmpty(IList<string> collection)
        {
            if (collection.Count > 0)
            {
                Assert.True(false, $"The collection was expected to be empty, but instead was [{Environment.NewLine}\"{String.Join($"\", {Environment.NewLine}\"", collection)}\"{Environment.NewLine}]");
            }
        }

        public static void StringEqual(string expected, string actual, bool ignoreCase = false)
        {
            var comparer = ignoreCase ? StringObjectEqualityComparer.InvariantCultureIgnoreCase : StringObjectEqualityComparer.InvariantCulture;
            Assert.Equal<object>(expected, actual, comparer);
        }

        public static void NotStringEqual(string expected, string actual, bool ignoreCase = false)
        {
            var comparer = ignoreCase ? StringObjectEqualityComparer.InvariantCultureIgnoreCase : StringObjectEqualityComparer.InvariantCulture;
            Assert.NotEqual<object>(expected, actual, comparer);
        }

        private class StringObjectEqualityComparer : IEqualityComparer<object>
        {
            public static readonly StringObjectEqualityComparer InvariantCultureIgnoreCase = new StringObjectEqualityComparer(true);
            public static readonly StringObjectEqualityComparer InvariantCulture = new StringObjectEqualityComparer(false);

            private readonly StringComparer stringComparer;

            public StringObjectEqualityComparer(bool ignoreCase)
            {
                this.stringComparer = ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
            }

            public new bool Equals(object x, object y)
            {
                return this.stringComparer.Equals((string)x,(string)y);
            }

            public int GetHashCode(object obj)
            {
                return this.stringComparer.GetHashCode((string)obj);
            }
        }

        // There appears to have been a bug in VC++, which might or might not have been partially
        // or completely corrected. It was unable to disambiguate a call to:
        //     Xunit::Assert::Throws(System::Type^, System::Action^)
        // from a call to:
        //     Xunit::Assert::Throws(System::Type^, System::Func<System::Object^>^)
        // that implicitly ignores its return value.
        //
        // The ambiguity may have been reported by some versions of the compiler and not by others.
        // Some versions of the compiler may not have emitted any code in this situation, making it
        // appear that the test has passed when, in fact, it hasn't been run.
        //
        // This situation is not an issue for C#.
        //
        // The following method is used to isolate DUtilTests in order to overcome the above problem.

        /// <summary>
        /// This shim allows C++/CLR code to call the Xunit method with the same signature
        /// without getting an ambiguous overload error.  If the specified test code
        /// fails to generate an exception of the exact specified type, an assertion
        /// exception is thrown. Otherwise, execution flow proceeds as normal.
        /// </summary>
        /// <typeparam name="T">The type name of the expected exception.</typeparam>
        /// <param name="testCode">An Action delegate to run the test code.</param>
        public static new void Throws<T>(System.Action testCode)
            where T : System.Exception
        {
            Xunit.Assert.Throws<T>(testCode);
        }

        // This shim has been tested, but is not currently used anywhere. It was provided
        // at the same time as the preceding shim because it involved the same overload
        // resolution conflict.

        /// <summary>
        /// This shim allows C++/CLR code to call the Xunit method with the same signature
        /// without getting an ambiguous overload error.  If the specified test code
        /// fails to generate an exception of the exact specified type, an assertion
        /// exception is thrown. Otherwise, execution flow proceeds as normal.
        /// </summary>
        /// <param name="exceptionType">The type object associated with exceptions of the expected type.</param>
        /// <param name="testCode">An Action delegate to run the test code.</param>
        /// <returns>An exception of a type other than the type specified, is such an exception is thrown.</returns>
        public static new System.Exception Throws(System.Type exceptionType, System.Action testCode)
        {
            return Xunit.Assert.Throws(exceptionType, testCode);
        }
    }
}
