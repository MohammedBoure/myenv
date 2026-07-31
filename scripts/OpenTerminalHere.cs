using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyEnv {
    class Program {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public const uint GW_HWNDNEXT = 2;

        static void Main(string[] args) {
            string terminal = (args.Length > 0 && args[0].ToLower() == "cmd") ? "cmd.exe" : "powershell.exe";
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string targetPath = userProfile;

            try {
                IntPtr activeHwnd = FindActiveExplorerHwnd();
                string clsName = GetWindowClassName(activeHwnd);

                if (clsName == "Progman" || clsName == "WorkerW") {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    if (Directory.Exists(desktop)) {
                        targetPath = desktop;
                    }
                } else if (activeHwnd != IntPtr.Zero) {
                    Type shellType = Type.GetTypeFromProgID("Shell.Application");
                    if (shellType != null) {
                        dynamic shell = Activator.CreateInstance(shellType);
                        dynamic windows = shell.Windows();

                        foreach (dynamic w in windows) {
                            try {
                                long wHwnd = Convert.ToInt64(w.HWND);
                                long targetHwnd = activeHwnd.ToInt64();

                                if (wHwnd == targetHwnd) {
                                    string p = w.Document.Folder.Self.Path;
                                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p)) {
                                        targetPath = p;
                                        break;
                                    }
                                }
                            } catch {}
                        }
                    }
                }
            } catch {}

            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = terminal,
                WorkingDirectory = targetPath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        static IntPtr FindActiveExplorerHwnd() {
            IntPtr current = GetForegroundWindow();
            int maxDepth = 100;
            IntPtr desktopHwnd = IntPtr.Zero;

            while (current != IntPtr.Zero && maxDepth > 0) {
                if (IsWindowVisible(current)) {
                    StringBuilder sb = new StringBuilder(256);
                    GetClassName(current, sb, sb.Capacity);
                    string cls = sb.ToString();

                    if (cls == "CabinetWClass" || cls == "ExploreWClass") {
                        return current;
                    }
                    if (cls == "Progman" || cls == "WorkerW") {
                        if (desktopHwnd == IntPtr.Zero) desktopHwnd = current;
                    }
                }
                current = GetWindow(current, GW_HWNDNEXT);
                maxDepth--;
            }
            return desktopHwnd;
        }

        static string GetWindowClassName(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero) return "";
            StringBuilder sb = new StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
