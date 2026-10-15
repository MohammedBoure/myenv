using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

// ==============================================================================
// MyEnv.Helpers - High-performance static helpers for PowerShell
// Precompiled to eliminate runtime Add-Type compilation overhead.
// ==============================================================================

public static class FolderSizeEngine {
    public static string FormatSize(long bytes) {
        if (bytes >= 1099511627776L) return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} TB", bytes / 1099511627776.0);
        if (bytes >= 1073741824L) return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} GB", bytes / 1073741824.0);
        if (bytes >= 1048576L) return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} MB", bytes / 1048576.0);
        if (bytes >= 1024L) return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} KB", bytes / 1024.0);
        return bytes + " B";
    }

    public static long MeasureWithProgress(
        string path,
        int linesUp,
        int col,
        bool updateLive,
        int maxDepth = 25)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return 0;

        long total = 0;
        var stack = new Stack<Tuple<string, int>>();
        stack.Push(new Tuple<string, int>(path, 0));

        int lastTick = Environment.TickCount;
        long lastReportedTotal = -1;

        if (updateLive) {
            string initial = FormatSize(0).PadLeft(14);
            try {
                Console.Write(string.Format("\x1b[{0}A\x1b[{1}G\x1b[33m{2}\x1b[0m\x1b[{0}B\x1b[1G", linesUp, col, initial));
            } catch {}
        }

        while (stack.Count > 0) {
            try {
                if (Console.KeyAvailable) break;
            } catch {}

            var current = stack.Pop();
            string currentDir = current.Item1;
            int depth = current.Item2;

            try {
                var dirInfo = new DirectoryInfo(currentDir);
                if ((dirInfo.Attributes & FileAttributes.ReparsePoint) != 0 && depth > 0) continue;

                foreach (var file in dirInfo.EnumerateFiles()) {
                    try {
                        total += file.Length;
                    } catch {}

                    int now = Environment.TickCount;
                    if (updateLive && (now - lastTick >= 35) && total != lastReportedTotal) {
                        lastTick = now;
                        lastReportedTotal = total;
                        string formatted = FormatSize(total).PadLeft(14);
                        try {
                            Console.Write(string.Format("\x1b[{0}A\x1b[{1}G\x1b[33m{2}\x1b[0m\x1b[{0}B\x1b[1G", linesUp, col, formatted));
                        } catch {}
                        Thread.Sleep(3);
                    }
                }

                if (depth < maxDepth) {
                    foreach (var subDir in dirInfo.EnumerateDirectories()) {
                        try {
                            if ((subDir.Attributes & FileAttributes.ReparsePoint) == 0) {
                                stack.Push(new Tuple<string, int>(subDir.FullName, depth + 1));
                            }
                        } catch {}
                    }
                }
            } catch {}
        }

        if (updateLive) {
            string finalFormatted = FormatSize(total).PadLeft(14);
            try {
                Console.Write(string.Format("\x1b[{0}A\x1b[{1}G\x1b[96m{2}\x1b[0m\x1b[{0}B\x1b[1G", linesUp, col, finalFormatted));
            } catch {}
        }

        return total;
    }
}

public class AgyCredVault {
    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredDelete(string target, int type, int reservedFlag);

    [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    public static extern void CredFree(IntPtr cred);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL {
        public int Flags;
        public int Type;
        public string TargetName;
        public string Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    public static string Read(string target) {
        IntPtr p;
        if (!CredRead(target, 1, 0, out p)) return null;
        try {
            var c = (CREDENTIAL)Marshal.PtrToStructure(p, typeof(CREDENTIAL));
            if (c.CredentialBlobSize <= 0 || c.CredentialBlob == IntPtr.Zero) return string.Empty;
            byte[] b = new byte[c.CredentialBlobSize];
            Marshal.Copy(c.CredentialBlob, b, 0, c.CredentialBlobSize);
            if (b.Length >= 2 && b[1] == 0) {
                return Encoding.Unicode.GetString(b);
            }
            return Encoding.UTF8.GetString(b);
        } finally {
            CredFree(p);
        }
    }

    public static bool Write(string target, string userName, string password) {
        if (password == null) password = string.Empty;
        // Antigravity CLI (Go keyring) requires UTF-8 byte stream
        byte[] blob = Encoding.UTF8.GetBytes(password);
        IntPtr blobPtr = Marshal.AllocHGlobal(blob.Length);
        try {
            Marshal.Copy(blob, 0, blobPtr, blob.Length);
            CREDENTIAL cred = new CREDENTIAL();
            cred.Type = 1; // Generic
            cred.TargetName = target;
            cred.UserName = userName;
            cred.CredentialBlobSize = blob.Length;
            cred.CredentialBlob = blobPtr;
            cred.Persist = 2; // Local machine
            return CredWrite(ref cred, 0);
        } finally {
            Marshal.FreeHGlobal(blobPtr);
        }
    }

    public static bool Delete(string target) {
        return CredDelete(target, 1, 0);
    }
}
