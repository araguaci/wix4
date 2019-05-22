// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Data
{
    using WixToolset.Data.Tuples;

    public static partial class TupleDefinitions
    {
        public static readonly IntermediateTupleDefinition MsiShortcutProperty = new IntermediateTupleDefinition(
            TupleDefinitionType.MsiShortcutProperty,
            new[]
            {
                new IntermediateFieldDefinition(nameof(MsiShortcutPropertyTupleFields.ShortcutRef), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(MsiShortcutPropertyTupleFields.PropertyKey), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(MsiShortcutPropertyTupleFields.PropVariantValue), IntermediateFieldType.String),
            },
            typeof(MsiShortcutPropertyTuple));
    }
}

namespace WixToolset.Data.Tuples
{
    public enum MsiShortcutPropertyTupleFields
    {
        ShortcutRef,
        PropertyKey,
        PropVariantValue,
    }

    public class MsiShortcutPropertyTuple : IntermediateTuple
    {
        public MsiShortcutPropertyTuple() : base(TupleDefinitions.MsiShortcutProperty, null, null)
        {
        }

        public MsiShortcutPropertyTuple(SourceLineNumber sourceLineNumber, Identifier id = null) : base(TupleDefinitions.MsiShortcutProperty, sourceLineNumber, id)
        {
        }

        public IntermediateField this[MsiShortcutPropertyTupleFields index] => this.Fields[(int)index];

        public string ShortcutRef
        {
            get => (string)this.Fields[(int)MsiShortcutPropertyTupleFields.ShortcutRef];
            set => this.Set((int)MsiShortcutPropertyTupleFields.ShortcutRef, value);
        }

        public string PropertyKey
        {
            get => (string)this.Fields[(int)MsiShortcutPropertyTupleFields.PropertyKey];
            set => this.Set((int)MsiShortcutPropertyTupleFields.PropertyKey, value);
        }

        public string PropVariantValue
        {
            get => (string)this.Fields[(int)MsiShortcutPropertyTupleFields.PropVariantValue];
            set => this.Set((int)MsiShortcutPropertyTupleFields.PropVariantValue, value);
        }
    }
}