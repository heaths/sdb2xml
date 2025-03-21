// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Win32.SafeHandles;

namespace sdb2xml
{
    internal class Pdb : SafeHandleMinusOneIsInvalid
    {
        private Pdb() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.SdbCloseDatabase(handle);
            return true;
        }
    }
}
