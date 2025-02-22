// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.CommandLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using WixToolset.Extensibility;
    using WixToolset.Extensibility.Data;
    using WixToolset.Extensibility.Services;

    internal class HelpCommand : ICommandLineCommand
    {
        private static readonly ExtensionCommandLineSwitch[] BuiltInSwitches = new ExtensionCommandLineSwitch[]
        {
            new ExtensionCommandLineSwitch { Switch = "build", Description = "Build a wixlib, package or bundle." },
            new ExtensionCommandLineSwitch { Switch = "decompile", Description = "Decompile a package or bundle into source code." },
        };

        public HelpCommand(IEnumerable<IExtensionCommandLine> extensions, IWixBranding branding)
        {
            this.Extensions = extensions;
            this.Branding = branding;
        }

        public bool ShowHelp
        {
            get => true;
            set { }
        }

        public bool ShowLogo
        {
            get => true;
            set { }
        }

        public bool StopParsing => true;

        private IEnumerable<IExtensionCommandLine> Extensions { get; }

        private IWixBranding Branding { get; }

        public Task<int> ExecuteAsync(CancellationToken _)
        {
            var commandLineSwitches = new List<ExtensionCommandLineSwitch>(BuiltInSwitches);
            commandLineSwitches.AddRange(this.Extensions.SelectMany(e => e.CommandLineSwitches).OrderBy(s => s.Switch, StringComparer.Ordinal));

            Console.WriteLine();
            Console.WriteLine("Usage: wix [option]");
            Console.WriteLine("Usage: wix [command]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h|--help         Show command line help.");
            Console.WriteLine("  --version         Display WiX Toolset version in use.");
            Console.WriteLine();

            Console.WriteLine("Commands:");
            foreach (var commandLineSwitch in commandLineSwitches)
            {
                Console.WriteLine("  {0,-17} {1}", commandLineSwitch.Switch, commandLineSwitch.Description);
            }

            Console.WriteLine();
            Console.WriteLine("Run 'wix [command] -h[elp]' for more information on a command.");
            Console.WriteLine();
            Console.WriteLine(this.Branding.ReplacePlaceholders("For more information see: [SupportUrl]"));

            return Task.FromResult(-1);
        }

        public bool TryParseArgument(ICommandLineParser parseHelper, string argument)
        {
            return true; // eat any arguments
        }
    }
}
