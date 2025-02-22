// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Example.Extension;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.Burn;
    using WixToolset.Core.TestPackage;
    using WixToolset.Data;
    using WixToolset.Data.Burn;
    using WixToolset.Data.Symbols;
    using WixToolset.Dtf.Resources;
    using Xunit;

    public class BundleFixture
    {
        [Fact]
        public void CanBuildMultiFileBundle()
        {
            var folder = TestData.Get(@"TestData\SimpleBundle");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "MultiFileBootstrapperApplication.wxs"),
                    Path.Combine(folder, "MultiFileBundle.wxs"),
                    "-loc", Path.Combine(folder, "Bundle.en-us.wxl"),
                    "-bindpath", Path.Combine(folder, "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, @"bin\test.exe")
                });

                result.AssertSuccess();

                Assert.True(File.Exists(Path.Combine(baseFolder, @"bin\test.exe")));
                Assert.True(File.Exists(Path.Combine(baseFolder, @"bin\test.wixpdb")));
            }
        }

        [Fact]
        public void CanBuildSimpleBundle()
        {
            var folder = TestData.Get(@"TestData\SimpleBundle");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                var pdbPath = Path.Combine(baseFolder, @"bin\test.wixpdb");
                var baFolderPath = Path.Combine(baseFolder, "ba");
                var extractFolderPath = Path.Combine(baseFolder, "extract");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Bundle.wxs"),
                    "-loc", Path.Combine(folder, "Bundle.en-us.wxl"),
                    "-bindpath", Path.Combine(folder, "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();
                Assert.Empty(result.Messages.Where(m => m.Level == MessageLevel.Warning));

                Assert.True(File.Exists(exePath));
                Assert.True(File.Exists(pdbPath));

                using (var wixOutput = WixOutput.Read(pdbPath))
                {

                    var intermediate = Intermediate.Load(wixOutput);
                    var section = intermediate.Sections.Single();

                    var bundleSymbol = section.Symbols.OfType<WixBundleSymbol>().Single();
                    Assert.Equal("1.0.0.0", bundleSymbol.Version);

                    var previousVersion = bundleSymbol.Fields[(int)WixBundleSymbolFields.Version].PreviousValue;
                    Assert.Equal("!(bind.packageVersion.test.msi)", previousVersion.AsString());

                    var msiSymbol = section.Symbols.OfType<WixBundlePackageSymbol>().Single();
                    Assert.Equal("test.msi", msiSymbol.Id.Id);

                    var extractResult = BundleExtractor.ExtractBAContainer(null, exePath, baFolderPath, extractFolderPath);
                    extractResult.AssertSuccess();

                    var burnManifestData = wixOutput.GetData(BurnConstants.BurnManifestWixOutputStreamName);
                    var extractedBurnManifestData = File.ReadAllText(Path.Combine(baFolderPath, "manifest.xml"), Encoding.UTF8);
                    Assert.Equal(extractedBurnManifestData, burnManifestData);

                    var baManifestData = wixOutput.GetData(BurnConstants.BootstrapperApplicationDataWixOutputStreamName);
                    var extractedBaManifestData = File.ReadAllText(Path.Combine(baFolderPath, "BootstrapperApplicationData.xml"), Encoding.UTF8);
                    Assert.Equal(extractedBaManifestData, baManifestData);

                    var bextManifestData = wixOutput.GetData(BurnConstants.BundleExtensionDataWixOutputStreamName);
                    var extractedBextManifestData = File.ReadAllText(Path.Combine(baFolderPath, "BundleExtensionData.xml"), Encoding.UTF8);
                    Assert.Equal(extractedBextManifestData, bextManifestData);

                    foreach (XmlAttribute attribute in extractResult.ManifestDocument.DocumentElement.Attributes)
                    {
                        switch (attribute.LocalName)
                        {
                            case "EngineVersion":
                                Assert.Equal($"{ThisAssembly.Git.BaseVersion.Major}.{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}.{ThisAssembly.Git.Commits}", attribute.Value);
                                break;
                            case "ProtocolVersion":
                                Assert.Equal("1", attribute.Value);
                                break;
                            case "Win64":
                                Assert.Equal("no", attribute.Value);
                                break;
                            case "xmlns":
                                Assert.Equal("http://wixtoolset.org/schemas/v4/2008/Burn", attribute.Value);
                                break;
                            default:
                                Assert.False(true, $"Attribute: '{attribute.LocalName}', Value: '{attribute.Value}'");
                                break;
                        }
                    }

                    var commandLineElements = extractResult.SelectManifestNodes("/burn:BurnManifest/burn:CommandLine");
                    var commandLineElement = (XmlNode)Assert.Single(commandLineElements);
                    Assert.Equal("<CommandLine Variables='upperCase' />", commandLineElement.GetTestXml());

                    var logElements = extractResult.SelectManifestNodes("/burn:BurnManifest/burn:Log");
                    var logElement = (XmlNode)Assert.Single(logElements);
                    Assert.Equal("<Log PathVariable='WixBundleLog' Prefix='~TestBundle' Extension='log' />", logElement.GetTestXml());

                    var registrationElements = extractResult.SelectManifestNodes("/burn:BurnManifest/burn:Registration");
                    var registrationElement = (XmlNode)Assert.Single(registrationElements);
                    Assert.Equal($"<Registration Id='{bundleSymbol.BundleId}' ExecutableName='test.exe' PerMachine='yes' Tag='' Version='1.0.0.0' ProviderKey='{bundleSymbol.BundleId}'>" +
                        "<Arp DisplayName='~TestBundle' DisplayVersion='1.0.0.0' InProgressDisplayName='~InProgressTestBundle' Publisher='Example Corporation' />" +
                        "</Registration>", registrationElement.GetTestXml());

                    var msiPayloads = extractResult.SelectManifestNodes("/burn:BurnManifest/burn:Payload[@Id='test.msi']");
                    var msiPayload = (XmlNode)Assert.Single(msiPayloads);
                    Assert.Equal("<Payload Id='test.msi' FilePath='test.msi' FileSize='*' Hash='*' Packaging='embedded' SourcePath='a0' Container='WixAttachedContainer' />",
                        msiPayload.GetTestXml(new Dictionary<string, List<string>>() { { "Payload", new List<string> { "FileSize", "Hash" } } }));
                }

                var manifestResource = new Resource(ResourceType.Manifest, "#1", 1033);
                manifestResource.Load(exePath);
                var actualManifestData = Encoding.UTF8.GetString(manifestResource.Data);
                Assert.Equal("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\">" +
                    "<assemblyIdentity name=\"test.exe\" version=\"1.0.0.0\" processorArchitecture=\"x86\" type=\"win32\" />" +
                    "<description>~TestBundle</description>" +
                    "<dependency><dependentAssembly><assemblyIdentity name=\"Microsoft.Windows.Common-Controls\" version=\"6.0.0.0\" processorArchitecture=\"x86\" publicKeyToken=\"6595b64144ccf1df\" language=\"*\" type=\"win32\" /></dependentAssembly></dependency>" +
                    "<compatibility xmlns=\"urn:schemas-microsoft-com:compatibility.v1\"><application><supportedOS Id=\"{e2011457-1546-43c5-a5fe-008deee3d3f0}\" /><supportedOS Id=\"{35138b9a-5d96-4fbd-8e2d-a2440225f93a}\" /><supportedOS Id=\"{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}\" /><supportedOS Id=\"{1f676c76-80e1-4239-95bb-83d0f6d0da78}\" /><supportedOS Id=\"{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}\" /></application></compatibility>" +
                    "<trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v3\"><security><requestedPrivileges><requestedExecutionLevel level=\"asInvoker\" uiAccess=\"false\" /></requestedPrivileges></security></trustInfo>" +
                    "<application xmlns=\"urn:schemas-microsoft-com:asm.v3\"><windowsSettings><dpiAware xmlns=\"http://schemas.microsoft.com/SMI/2005/WindowsSettings\">true/pm</dpiAware><dpiAwareness xmlns=\"http://schemas.microsoft.com/SMI/2016/WindowsSettings\">PerMonitorV2, PerMonitor</dpiAwareness></windowsSettings></application>" +
                    "</assembly>", actualManifestData);
            }
        }

        [Fact]
        public void CanBuildX64Bundle()
        {
            var folder = TestData.Get(@"TestData\SimpleBundle");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                var pdbPath = Path.Combine(baseFolder, @"bin\test.wixpdb");
                var baFolderPath = Path.Combine(baseFolder, "ba");
                var attachedFolderPath = Path.Combine(baseFolder, "attached");
                var extractFolderPath = Path.Combine(baseFolder, "extract");

                var result = WixRunner.Execute(false, new[] // TODO: go back to elevating warnings as errors.
                {
                    "build",
                    "-arch", "x64",
                    Path.Combine(folder, "Bundle.wxs"),
                    "-loc", Path.Combine(folder, "Bundle.en-us.wxl"),
                    "-bindpath", Path.Combine(folder, "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(exePath));
                Assert.True(File.Exists(pdbPath));

                var manifestResource = new Resource(ResourceType.Manifest, "#1", 1033);
                manifestResource.Load(exePath);
                var actualManifestData = Encoding.UTF8.GetString(manifestResource.Data);
                Assert.Equal("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\">" +
                    "<assemblyIdentity name=\"test.exe\" version=\"1.0.0.0\" processorArchitecture=\"amd64\" type=\"win32\" />" +
                    "<description>~TestBundle</description>" +
                    "<dependency><dependentAssembly><assemblyIdentity name=\"Microsoft.Windows.Common-Controls\" version=\"6.0.0.0\" processorArchitecture=\"amd64\" publicKeyToken=\"6595b64144ccf1df\" language=\"*\" type=\"win32\" /></dependentAssembly></dependency>" +
                    "<compatibility xmlns=\"urn:schemas-microsoft-com:compatibility.v1\"><application><supportedOS Id=\"{e2011457-1546-43c5-a5fe-008deee3d3f0}\" /><supportedOS Id=\"{35138b9a-5d96-4fbd-8e2d-a2440225f93a}\" /><supportedOS Id=\"{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}\" /><supportedOS Id=\"{1f676c76-80e1-4239-95bb-83d0f6d0da78}\" /><supportedOS Id=\"{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}\" /></application></compatibility>" +
                    "<trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v3\"><security><requestedPrivileges><requestedExecutionLevel level=\"asInvoker\" uiAccess=\"false\" /></requestedPrivileges></security></trustInfo>" +
                    "<application xmlns=\"urn:schemas-microsoft-com:asm.v3\"><windowsSettings><dpiAware xmlns=\"http://schemas.microsoft.com/SMI/2005/WindowsSettings\">true/pm</dpiAware><dpiAwareness xmlns=\"http://schemas.microsoft.com/SMI/2016/WindowsSettings\">PerMonitorV2, PerMonitor</dpiAwareness></windowsSettings></application>" +
                    "</assembly>", actualManifestData);

                var extractResult = BundleExtractor.ExtractAllContainers(null, exePath, baFolderPath, attachedFolderPath, extractFolderPath);
                extractResult.AssertSuccess();

                foreach (XmlAttribute attribute in extractResult.ManifestDocument.DocumentElement.Attributes)
                {
                    switch (attribute.LocalName)
                    {
                        case "EngineVersion":
                            Assert.Equal($"{ThisAssembly.Git.BaseVersion.Major}.{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}.{ThisAssembly.Git.Commits}", attribute.Value);
                            break;
                        case "ProtocolVersion":
                            Assert.Equal("1", attribute.Value);
                            break;
                        case "Win64":
                            Assert.Equal("yes", attribute.Value);
                            break;
                        case "xmlns":
                            Assert.Equal("http://wixtoolset.org/schemas/v4/2008/Burn", attribute.Value);
                            break;
                        default:
                            Assert.False(true, $"Attribute: '{attribute.LocalName}', Value: '{attribute.Value}'");
                            break;
                    }
                }
            }
        }

        [Fact]
        public void CanBuildSimpleBundleUsingExtensionBA()
        {
            var extensionPath = Path.GetFullPath(new Uri(typeof(ExampleExtensionFactory).Assembly.CodeBase).LocalPath);
            var folder = TestData.Get(@"TestData\SimpleBundle");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "MultiFileBundle.wxs"),
                    "-loc", Path.Combine(folder, "Bundle.en-us.wxl"),
                    "-ext", extensionPath,
                    "-bindpath", Path.Combine(folder, "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, @"bin\test.exe")
                });

                result.AssertSuccess();

                Assert.True(File.Exists(Path.Combine(baseFolder, @"bin\test.exe")));
                Assert.True(File.Exists(Path.Combine(baseFolder, @"bin\test.wixpdb")));
            }
        }

        [Fact]
        public void CanBuildSingleExeBundle()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SingleExeBundle", "SingleExePackageGroup.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(exePath));
            }
        }

        [Fact]
        public void CanBuildSingleExeRemotePayloadBundle()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                var pdbPath = Path.Combine(baseFolder, @"bin\test.wixpdb");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SingleExeBundle", "SingleExeRemotePayload.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(exePath));
                Assert.True(File.Exists(pdbPath));

                using (var wixOutput = WixOutput.Read(pdbPath))
                {
                    var intermediate = Intermediate.Load(wixOutput);
                    var section = intermediate.Sections.Single();

                    var packageSymbol = section.Symbols.OfType<WixBundlePackageSymbol>().Where(x => x.Id.Id == "NetFx462Web").Single();
                    Assert.Equal(Int64.MaxValue, packageSymbol.InstallSize);

                    var payloadSymbol = section.Symbols.OfType<WixBundlePayloadSymbol>().Where(x => x.Id.Id == "NetFx462Web").Single();
                    Assert.Equal(Int64.MaxValue, payloadSymbol.FileSize);
                }
            }
        }

        [Fact]
        public void CannotBuildBundleWithInvalidIcon()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleWithInvalid", "BundleWithInvalidIcon.wxs"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, @"bin\test.exe")
                });

                var message = result.Messages.Where(m => m.Level == MessageLevel.Error).Select(m => m.ToString().Replace(folder, "<testdata>")).ToArray();
                WixAssert.CompareLineByLine(new[]
                {
                    @"Failed to add resources to the bundle. Ensure the bundle icon file is an icon file at '<testdata>\.Data\burn.exe'"
                }, message);
            }
        }

        [Fact]
        public void CannotBuildBundleWithInvalidUpgradeCode()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleLocalized", "BundleWithLocalizedUpgradeCode.wxs"),
                    "-loc", Path.Combine(folder, "BundleLocalized", "BundleWithInvalidUpgradeCode.wxl"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, @"bin\test.exe")
                });

                var message = result.Messages.Where(m => m.Level == MessageLevel.Error).Select(m => m.ToString().Replace(folder, "<testdata>")).ToArray();
                WixAssert.CompareLineByLine(new[]
                {
                    "The Bundle/@UpgradeCode attribute's value, 'NOT-A-GUID', is not a legal guid value."
                }, message);
            }
        }

        [Fact]
        public void CanBuildUncompressedBundle()
        {
            var folder = TestData.Get(@"TestData") + Path.DirectorySeparatorChar;

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder() + Path.DirectorySeparatorChar;
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                var trackingFile = Path.Combine(intermediateFolder, "trackingFile.txt");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleUncompressed", "UncompressedBundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                    "-trackingFile", trackingFile
                });

                result.AssertSuccess();

                Assert.True(File.Exists(exePath));
                Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(exePath), "test.txt")));

                var trackedLines = File.ReadAllLines(trackingFile).Select(s => s.Replace(baseFolder, null, StringComparison.OrdinalIgnoreCase).Replace(folder, null, StringComparison.OrdinalIgnoreCase)).ToArray();
                WixAssert.CompareLineByLine(new[]
                {
                    "BuiltOutput\tbin\\test.exe",
                    "BuiltOutput\tbin\\test.wixpdb",
                    "CopiedOutput\tbin\\test.txt",
                    "Input\tSimpleBundle\\data\\fakeba.dll",
                    "Input\tSimpleBundle\\data\\MsiPackage\\test.txt"
                }, trackedLines);
            }
        }

        [Fact]
        public void CantBuildWithDuplicateCacheIds()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "DuplicateCacheIds.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                Assert.Equal(8001, result.ExitCode);
            }
        }

        [Fact]
        public void CantBuildWithDuplicatePayloadNames()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "DuplicatePayloadNames.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                var attachedContainerWarnings = result.Messages.Where(m => m.Id == (int)BurnBackendWarnings.Ids.AttachedContainerPayloadCollision)
                                                               .Select(m => m.ToString())
                                                               .ToArray();
                WixAssert.CompareLineByLine(new string[]
                {
                    "The Payload 'Auto2' has a duplicate Name 'burn.exe' in the attached container. When extracting the bundle with dark.exe, the file will get overwritten.",
                }, attachedContainerWarnings);

                var baContainerErrors = result.Messages.Where(m => m.Id == (int)BurnBackendErrors.Ids.BAContainerPayloadCollision)
                                                       .Select(m => m.ToString())
                                                       .ToArray();
                WixAssert.CompareLineByLine(new string[]
                {
                    "The Payload 'DuplicatePayloadNames.wxs' has a duplicate Name 'fakeba.dll' in the BA container. When extracting the container at runtime, the file will get overwritten.",
                    "The Payload 'uxTxMXPVMXwQrPTMIGa5WGt93w0Ns' has a duplicate Name 'BootstrapperApplicationData.xml' in the BA container. When extracting the container at runtime, the file will get overwritten.",
                    "The Payload 'uxYRbgitOs0K878jn5L_z7LdJ21KI' has a duplicate Name 'BundleExtensionData.xml' in the BA container. When extracting the container at runtime, the file will get overwritten.",
                }, baContainerErrors);

                var externalErrors = result.Messages.Where(m => m.Id == (int)BurnBackendErrors.Ids.ExternalPayloadCollision)
                                                    .Select(m => m.ToString())
                                                    .ToArray();
                WixAssert.CompareLineByLine(new string[]
                {
                    "The external Payload 'HiddenPersistedBundleVariable.wxs' has a duplicate Name 'PayloadCollision'. When building the bundle or laying out the bundle, the file will get overwritten.",
                    "The external Container 'MsiPackagesContainer' has a duplicate Name 'ContainerCollision'. When building the bundle or laying out the bundle, the file will get overwritten.",
                }, externalErrors);

                var packageCacheErrors = result.Messages.Where(m => m.Id == (int)BurnBackendErrors.Ids.PackageCachePayloadCollision)
                                                        .Select(m => m.ToString())
                                                        .ToArray();
                WixAssert.CompareLineByLine(new string[]
                {
                    "The Payload 'test.msi' has a duplicate Name 'test.msi' in package 'test.msi'. When caching the package, the file will get overwritten.",
                }, packageCacheErrors);

                Assert.Equal(14, result.Messages.Length);
            }
        }

        [Fact]
        public void CantBuildWithOrphanPayload()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "OrphanPayload.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "MinimalPackageGroup.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                Assert.Equal(7000, result.ExitCode);
            }
        }

        [Fact]
        public void CantBuildWithPackageInMultipleContainers()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "PackageInMultipleContainers.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "MinimalPackageGroup.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                Assert.Equal(7001, result.ExitCode);
            }
        }

        [Fact]
        public void CanBuildWithSubfolderContainer()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin", "test.exe");
                var containerPath = Path.Combine(baseFolder, "bin", "Data", "c1");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Bundle", "SubfolderContainer.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(containerPath), $"Failed to find external container: {containerPath}");
            }
        }

        [Fact]
        public void CantBuildWithUnscheduledPackage()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "UnscheduledPackage.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                Assert.Equal(7003, result.ExitCode);
            }
        }

        [Fact]
        public void CantBuildWithUnscheduledRollbackBoundary()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BadInput", "UnscheduledRollbackBoundary.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                Assert.Equal(7004, result.ExitCode);
            }
        }
    }
}
