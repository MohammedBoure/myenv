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
            bool isCmd = args.Length > 0 && args[0].Equals("cmd", StringComparison.OrdinalIgnoreCase);
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

            string terminalExe = "powershell.exe";
            if (isCmd) {
                terminalExe = "cmd.exe";
            } else {
                string pwshPath = FindPwshPath();
                if (!string.IsNullOrEmpty(pwshPath)) {
                    terminalExe = pwshPath;
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = terminalExe,
                WorkingDirectory = targetPath,
                UseShellExecute = true
            };

            if (!isCmd) {
                psi.Arguments = "-NoLogo";
            }

            try {
                Process.Start(psi);
            } catch {
                ProcessStartInfo fallback = new ProcessStartInfo(isCmd ? "cmd.exe" : "powershell.exe") {
                    WorkingDirectory = targetPath,
                    UseShellExecute = true
                };
                Process.Start(fallback);
            }
        }

        static string FindPwshPath() {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pwshPf = Path.Combine(programFiles, @"PowerShell\7\pwsh.exe");
            if (File.Exists(pwshPf)) return pwshPf;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pwshApp = Path.Combine(localAppData, @"Microsoft\WindowsApps\pwsh.exe");
            if (File.Exists(pwshApp)) return pwshApp;

            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            string[] dirs = pathEnv.Split(';');
            foreach (string dir in dirs) {
                if (string.IsNullOrWhiteSpace(dir)) continue;
                string candidate = Path.Combine(dir.Trim(), "pwsh.exe");
                if (File.Exists(candidate)) return candidate;
            }

            return null;
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
