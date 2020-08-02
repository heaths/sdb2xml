// Copyright 2020 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace sdb2xml
{
    internal class ShimDatabase : IDisposable
    {
        internal static string PatchDirectory
        {
            get
            {
                var stringBuilder = new StringBuilder(260);
                NativeMethods.SdbGetAppPatchDir(IntPtr.Zero, stringBuilder, stringBuilder.Capacity);
                return stringBuilder.ToString();
            }
        }

        internal ShimDatabase(string path)
        {
            Path = path;
            if (!File.Exists(Path))
            {
                var text = System.IO.Path.Combine(ShimDatabase.PatchDirectory, Path);
                if (!File.Exists(text))
                {
                    throw new FileNotFoundException(null, Path);
                }
                Path = text;
            }
            Handle = NativeMethods.SdbOpenDatabase(path, NativeMethods.PATH_TYPE.DOS_PATH);
            if (Handle.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        ~ShimDatabase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Handle != null && !Handle.IsClosed)
            {
                Handle.Dispose();
            }
        }

        internal Pdb Handle { get; }

        internal string Path { get; }

        internal Tag Root
        {
            get
            {
                if (root == null)
                {
                    root = new Tag(Handle, 0, null);
                }
                return root;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private Tag root;
    }
}
