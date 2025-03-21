// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace sdb2xml
{
    internal static class NativeMethods
    {
        [DllImport(APPHELP, CharSet = CharSet.Unicode)]
        internal static extern Pdb SdbOpenDatabase(string pszPath, NativeMethods.PATH_TYPE eType);

        [DllImport(APPHELP)]
        internal static extern void SdbCloseDatabase(IntPtr pdb);

        [DllImport(APPHELP)]
        internal static extern int SdbGetFirstChild(Pdb pdb, int tiParent);

        [DllImport(APPHELP)]
        internal static extern int SdbGetNextChild(Pdb pdb, int tiParent, int tiPrev);

        [DllImport(APPHELP)]
        internal static extern int SdbFindFirstTag(Pdb pdb, int tiParent, short tTag);

        [DllImport(APPHELP, CharSet = CharSet.Unicode)]
        internal static extern int SdbGetAppPatchDir(IntPtr hSDB, [Out] StringBuilder szAppPatchDir, int cchSize);

        [DllImport(APPHELP, CharSet = CharSet.Unicode)]
        internal static extern IntPtr SdbTagToString(short tag);

        [DllImport(APPHELP)]
        internal static extern short SdbGetTagFromTagID(Pdb pdb, int tiWhich);

        [DllImport(APPHELP)]
        internal static extern byte SdbReadBYTETag(Pdb pdb, int tiWhich, byte bDefault);

        [DllImport(APPHELP)]
        internal static extern short SdbReadWORDTag(Pdb pdb, int tiWhich, short wDefault);

        [DllImport(APPHELP)]
        internal static extern int SdbReadDWORDTag(Pdb pdb, int tiWhich, int dwDefault);

        [DllImport(APPHELP)]
        internal static extern long SdbReadQWORDTag(Pdb pdb, int tiWhich, long qwDefault);

        [DllImport(APPHELP)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SdbReadBinaryTag(Pdb pdb, int tiWhich, [Out] byte[] pBuffer, int dwBufferSize);

        [DllImport(APPHELP)]
        internal static extern int SdbGetTagDataSize(Pdb pdb, int tiWhich);

        [DllImport(APPHELP, CharSet = CharSet.Unicode)]
        internal static extern string SdbGetStringTagPtr(Pdb pdb, int tiWhich);

        private const string APPHELP = "apphelp.dll";

        internal const int MAX_PATH = 260;

        internal const int ERROR_SUCCESS = 0;

        internal const int ERROR_INVALID_PARAMETER = 87;

        internal const short TAG_TYPE_NULL = 4096;

        internal const short TAG_TYPE_BYTE = 8192;

        internal const short TAG_TYPE_WORD = 12288;

        internal const short TAG_TYPE_DWORD = 16384;

        internal const short TAG_TYPE_QWORD = 20480;

        internal const short TAG_TYPE_STRINGREF = 24576;

        internal const short TAG_TYPE_LIST = 28672;

        internal const short TAG_TYPE_STRING = -32768;

        internal const short TAG_TYPE_BINARY = -28672;

        internal const short TAG_PATCH = 28677;

        internal const short TAG_FILE = 28684;

        internal const short TAG_DATA = 28687;

        internal const short TAG_NAME = 24577;

        internal const short TAG_TIME = 20481;

        internal const short TAG_PATCH_BITS = -28670;

        internal const short TAG_FILE_BITS = -28669;

        internal const short TAG_EXE_ID = -28668;

        internal const short TAG_DATA_BITS = -28667;

        internal const short TAG_MSI_PACKAGE_ID = -28666;

        internal const short TAG_DATABASE_ID = -28665;

        internal const int TAGID_ROOT = 0;

        internal enum PATH_TYPE
        {
            DOS_PATH,
            NT_PATH
        }
    }
}
