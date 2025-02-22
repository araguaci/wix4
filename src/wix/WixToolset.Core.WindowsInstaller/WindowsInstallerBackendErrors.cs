// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.WindowsInstaller
{
    using System;
    using WixToolset.Data;

    internal static class WindowsInstallerBackendErrors
    {
        public static Message CannotLoadWixoutAsTransform(SourceLineNumber sourceLineNumbers, Exception exception)
        {
            var additionalDetail = exception == null ? String.Empty : ", detail: " + exception.Message;

            return Message(sourceLineNumbers, Ids.CannotLoadWixoutAsTransform, "Could not load wixout file as a transform{1}", additionalDetail);
        }

        public static Message InvalidModuleVersion(SourceLineNumber originalLineNumber, string version)
        {
            return Message(originalLineNumber, Ids.InvalidModuleVersion, "The Module/@Version was not be able to be used as a four-part version. A valid four-part version has a max value of \"65535.65535.65535.65535\" and must be all numeric.", version);
        }

        private static Message Message(SourceLineNumber sourceLineNumber, Ids id, string format, params object[] args)
        {
            return new Message(sourceLineNumber, MessageLevel.Error, (int)id, format, args);
        }

        public enum Ids
        {
            CannotLoadWixoutAsTransform = 7500,
            InvalidModuleVersion = 7501,
        } // last available is 7999. 8000 is BurnBackendErrors.
    }
}
