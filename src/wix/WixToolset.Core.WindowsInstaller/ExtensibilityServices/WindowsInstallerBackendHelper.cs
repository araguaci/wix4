// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.WindowsInstaller.ExtensibilityServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WixToolset.Data;
    using WixToolset.Data.Symbols;
    using WixToolset.Data.WindowsInstaller;
    using WixToolset.Data.WindowsInstaller.Rows;
    using WixToolset.Extensibility.Data;
    using WixToolset.Extensibility.Services;

    internal class WindowsInstallerBackendHelper : IWindowsInstallerBackendHelper
    {
        private readonly IBackendHelper backendHelper;

        public WindowsInstallerBackendHelper(IServiceProvider serviceProvider)
        {
            this.backendHelper = serviceProvider.GetService<IBackendHelper>();
        }

        #region IBackendHelper interfaces

        public IFileFacade CreateFileFacade(FileSymbol file, AssemblySymbol assembly)
        {
            return this.backendHelper.CreateFileFacade(file, assembly);
        }

        public IFileFacade CreateFileFacade(FileRow fileRow)
        {
            return this.backendHelper.CreateFileFacade(fileRow);
        }

        public IFileFacade CreateFileFacadeFromMergeModule(FileSymbol fileSymbol)
        {
            return this.backendHelper.CreateFileFacadeFromMergeModule(fileSymbol);
        }

        public IFileTransfer CreateFileTransfer(string source, string destination, bool move, SourceLineNumber sourceLineNumbers = null)
        {
            return this.backendHelper.CreateFileTransfer(source, destination, move, sourceLineNumbers);
        }

        public string CreateGuid()
        {
            return this.backendHelper.CreateGuid();
        }

        public string CreateGuid(Guid namespaceGuid, string value)
        {
            return this.backendHelper.CreateGuid(namespaceGuid, value);
        }

        public IResolvedDirectory CreateResolvedDirectory(string directoryParent, string name)
        {
            return this.backendHelper.CreateResolvedDirectory(directoryParent, name);
        }

        public IReadOnlyList<ITrackedFile> ExtractEmbeddedFiles(IEnumerable<IExpectedExtractFile> embeddedFiles)
        {
            return this.backendHelper.ExtractEmbeddedFiles(embeddedFiles);
        }

        public string GenerateIdentifier(string prefix, params string[] args)
        {
            return this.backendHelper.GenerateIdentifier(prefix, args);
        }

        public int GetValidCodePage(string value, bool allowNoChange, bool onlyAnsi = false, SourceLineNumber sourceLineNumbers = null)
        {
            return this.backendHelper.GetValidCodePage(value, allowNoChange, onlyAnsi, sourceLineNumbers);
        }

        public string GetMsiFileName(string value, bool source, bool longName)
        {
            return this.backendHelper.GetMsiFileName(value, source, longName);
        }

        public bool IsValidBinderVariable(string variable)
        {
            return this.backendHelper.IsValidBinderVariable(variable);
        }

        public bool IsValidFourPartVersion(string version)
        {
            return this.backendHelper.IsValidFourPartVersion(version);
        }

        public bool IsValidIdentifier(string id)
        {
            return this.backendHelper.IsValidIdentifier(id);
        }

        public bool IsValidMsiProductVersion(string version)
        {
            return this.backendHelper.IsValidMsiProductVersion(version);
        }

        public bool IsValidWixVersion(string version)
        {
            return this.backendHelper.IsValidWixVersion(version);
        }

        public bool IsValidLongFilename(string filename, bool allowWildcards, bool allowRelative)
        {
            return this.backendHelper.IsValidLongFilename(filename, allowWildcards, allowRelative);
        }

        public bool IsValidShortFilename(string filename, bool allowWildcards)
        {
            return this.backendHelper.IsValidShortFilename(filename, allowWildcards);
        }

        public void ResolveDelayedFields(IEnumerable<IDelayedField> delayedFields, Dictionary<string, string> variableCache)
        {
            this.backendHelper.ResolveDelayedFields(delayedFields, variableCache);
        }

        public string[] SplitMsiFileName(string value)
        {
            return this.backendHelper.SplitMsiFileName(value);
        }

        public ITrackedFile TrackFile(string path, TrackedFileType type, SourceLineNumber sourceLineNumbers = null)
        {
            return this.backendHelper.TrackFile(path, type, sourceLineNumbers);
        }

        #endregion

        #region IWindowsInstallerBackendHelper interfaces

        public Row CreateRow(IntermediateSection section, IntermediateSymbol symbol, WindowsInstallerData data, TableDefinition tableDefinition)
        {
            var table = data.EnsureTable(tableDefinition);

            var row = table.CreateRow(symbol.SourceLineNumbers);
            row.SectionId = section.Id;

            return row;
        }

        public bool TryAddSymbolToMatchingTableDefinitions(IntermediateSection section, IntermediateSymbol symbol, WindowsInstallerData data, TableDefinitionCollection tableDefinitions)
        {
            var tableDefinition = tableDefinitions.FirstOrDefault(t => t.SymbolDefinition?.Name == symbol.Definition.Name);
            if (tableDefinition == null)
            {
                return false;
            }

            var row = this.CreateRow(section, symbol, data, tableDefinition);
            var rowOffset = 0;

            if (tableDefinition.SymbolIdIsPrimaryKey)
            {
                row[0] = symbol.Id.Id;
                rowOffset = 1;
            }

            for (var i = 0; i < symbol.Fields.Length; ++i)
            {
                if (i < tableDefinition.Columns.Length)
                {
                    var column = tableDefinition.Columns[i + rowOffset];

                    switch (column.Type)
                    {
                    case ColumnType.Number:
                        row[i + rowOffset] = column.Nullable ? symbol.AsNullableNumber(i) : symbol.AsNumber(i);
                        break;

                    default:
                        row[i + rowOffset] = symbol.AsString(i);
                        break;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
