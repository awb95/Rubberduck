﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using Rubberduck.Resources.Registration;

// ReSharper disable InconsistentNaming
// The parameters on RD's public interfaces are following VBA conventions not C# conventions to stop the
// obnoxious "Can I haz all identifiers with the same casing" behavior of the VBE.

namespace Rubberduck.UnitTesting
{
    internal class SetAttrParams : ISetAttrParams
    {
        public string PathName { get; } = nameof(PathName);

        public string Attributes { get; } = nameof(Attributes);
    }
}
