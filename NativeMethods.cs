using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;

namespace LinkTool
{
    public class NativeMethods
    {
            private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            private const uint FILE_READ_EA = 0x0008;
            private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CreateFile(
                    [MarshalAs(UnmanagedType.LPTStr)] string filename,
                    [MarshalAs(UnmanagedType.U4)] uint access,
                    [MarshalAs(UnmanagedType.U4)] FileShare share,
                    IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
                    [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                    [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
                    IntPtr templateFile);

            public static string GetFinalPathName(string path)
            {
                var h = CreateFile(path,
                    FILE_READ_EA,
                    FileShare.ReadWrite | FileShare.Delete,
                    IntPtr.Zero,
                    FileMode.Open,
                    FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero);
                if (h == INVALID_HANDLE_VALUE)
                    throw new Win32Exception();

                try
                {
                    var sb = new StringBuilder(1024);
                    var res = GetFinalPathNameByHandle(h, sb, 1024, 0);
                    if (res == 0)
                        throw new Win32Exception();

                    return sb.ToString();
                }
                finally
                {
                    CloseHandle(h);
                }
            }

        public enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
    }
}
