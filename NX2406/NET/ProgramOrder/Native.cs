﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace ProgramOrder {
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    internal sealed class NaturalStringComparer : IComparer<string> {
        public int Compare(string x, string y) => SafeNativeMethods.StrCmpLogicalW(x, y);
    }
}
