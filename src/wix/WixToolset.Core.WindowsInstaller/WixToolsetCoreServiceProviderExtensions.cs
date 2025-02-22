// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.WindowsInstaller
{
    using System;
    using System.Collections.Generic;
    using WixToolset.Core.WindowsInstaller.ExtensibilityServices;
    using WixToolset.Extensibility.Data;
    using WixToolset.Extensibility.Services;

    /// <summary>
    /// Extensions methods for adding WindowsInstaller services.
    /// </summary>
    public static class WixToolsetCoreServiceProviderExtensions
    {
        /// <summary>
        /// Adds WindowsInstaller services.
        /// </summary>
        /// <param name="coreProvider">Core service provider.</param>
        /// <returns>The core service provider provided to this method.</returns>
        public static IWixToolsetCoreServiceProvider AddWindowsInstallerBackend(this IWixToolsetCoreServiceProvider coreProvider)
        {
            AddServices(coreProvider);

            var extensionManager = coreProvider.GetService<IExtensionManager>();
            extensionManager.Add(typeof(WindowsInstallerExtensionFactory).Assembly);

            return coreProvider;
        }

        private static void AddServices(IWixToolsetCoreServiceProvider coreProvider)
        {
            // Singletons.
            coreProvider.AddService((provider, singletons) => AddSingleton<IWindowsInstallerBackendHelper>(singletons, new WindowsInstallerBackendHelper(provider)));
            coreProvider.AddService((provider, singletons) => AddSingleton<IWindowsInstallerDecompilerHelper>(singletons, new WindowsInstallerDecompilerHelper(provider)));

            // Transients.
            coreProvider.AddService<IWindowsInstallerDecompiler>((provider, singletons) => new WindowsInstallerDecompiler(provider));
            coreProvider.AddService<IWindowsInstallerDecompileContext>((provider, singletons) => new WindowsInstallerDecompileContext(provider));
            coreProvider.AddService<IWindowsInstallerDecompileResult>((provider, singletons) => new WindowsInstallerDecompileResult());
        }

        private static T AddSingleton<T>(Dictionary<Type, object> singletons, T service) where T : class
        {
            singletons.Add(typeof(T), service);
            return service;
        }
    }
}
