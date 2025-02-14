// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System.IO;
    using System.Linq;
    using System.Xml;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.TestPackage;
    using WixToolset.Data;
    using Xunit;

    public class VersionFixture
    {
        [Fact]
        public void CannotBuildMsiWithInvalidMajorVersion()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, @"bin\test1.msi");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Version", "Package.wxs"),
                    "-bindpath", Path.Combine(folder, "SingleFile", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-d", "Version=257.0.0",
                    "-o", msiPath
                });

                var message = result.Messages.Single(m => m.Level == MessageLevel.Error);
                Assert.Equal("Invalid product version '257.0.0'. Product version must have a major version less than 256, a minor version less than 256, and a build version less than 65536.", message.ToString());
                Assert.Equal(242, result.ExitCode);
            }
        }

        [Fact]
        public void CanBuildBundleWithSemanticVersion()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, @"bin\test1.msi");
                var msi2Path = Path.Combine(baseFolder, @"bin\test2.msi");
                var bundlePath = Path.Combine(baseFolder, @"bin\bundle.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Version", "Package.wxs"),
                    "-bindpath", Path.Combine(folder, "SingleFile", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-d", "Version=255.255.65535",
                    "-o", msiPath
                });

                result.AssertSuccess();

                var result3 = WixRunner.Execute(new[]
{
                    "build",
                    Path.Combine(folder, "Version", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(baseFolder, "bin"),
                    "-intermediateFolder", intermediateFolder,
                    "-d", "Version=2022.3.9-preview.0-build.5+0987654321abcdef1234567890",
                    "-o", bundlePath
                });

                result3.AssertSuccess();

                var propertyTable = Query.QueryDatabase(msiPath, new[] { "Property" }).Select(r => r.Split('\t')).ToDictionary(r => r[0].Substring("Property:".Length), r => r[1]);
                Assert.True(propertyTable.TryGetValue("ProductVersion", out var productVersion));
                Assert.Equal("255.255.65535", productVersion);

                var extractResult = BundleExtractor.ExtractAllContainers(null, bundlePath, Path.Combine(baseFolder, "ba"), Path.Combine(baseFolder, "attached"), Path.Combine(baseFolder, "extract"));
                extractResult.AssertSuccess();

                var bundleVersion = extractResult.SelectManifestNodes("/burn:BurnManifest/burn:Registration/@Version")
                                                 .Cast<XmlAttribute>()
                                                 .Single();
                Assert.Equal("2022.3.9-preview.0-build.5+0987654321abcdef1234567890", bundleVersion.Value);
            }
        }
    }
}
