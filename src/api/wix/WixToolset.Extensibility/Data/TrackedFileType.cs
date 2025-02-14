// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility.Data
{
    /// <summary>
    /// Tracked file types.
    /// </summary>
    public enum TrackedFileType
    {
        /// <summary>
        /// File tracked as input (like content included in an .msi).
        /// </summary>
        Input,

        /// <summary>
        /// Temporary file (like an .idt or any other temporary file).
        /// These are to be deleted before the build completes.
        /// </summary>
        Temporary,

        /// <summary>
        /// Intermediate file (like a .cab in the cabcache).
        /// These are left for subsequent builds.
        /// </summary>
        Intermediate,

        /// <summary>
        /// Output created by the build process itself (like a .msi, .cab or .wixpdb).
        /// These files can be recreated in the final output location by building again.
        /// </summary>
        BuiltOutput,

        /// <summary>
        /// Output copied by the build process (like external files in an .msi).
        /// These files are not created by the build process but are copied to the final output location.
        /// </summary>
        CopiedOutput,
    }
}
