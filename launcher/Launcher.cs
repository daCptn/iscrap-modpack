// iScrap Mod-Pack Launcher — GTA IV Complete Edition
// - Findet das Spiel automatisch (Steam-Registry + Bibliotheken), Pfad manuell aenderbar
// - Deinstalliert bestehende Mod-Installationen sauber (Saves bleiben laut keep.txt)
// - Installiert das Kern-Pack aus dem Git-Repo (fetch + version.txt-Vergleich)
// - Startet das Spiel (PlayGTAIV.exe / LaunchGTAIV.exe / GTAIV.exe)
// Build:
//   csc -nologo -target:winexe -platform:anycpu -optimize+ -out:Launcher.exe ^
//     -r:System.Windows.Forms.dll -r:System.Drawing.dll Launcher.cs
// Sicherheit: Es gibt keine Kommandozeilen-Strings mit Variablen. Git laeuft ueber
// konstante Batch-Dateien (nur feste Befehle, Arbeitsordner via %~dp0), gestartet mit
// ShellExecuteEx (P/Invoke); variable Werte (Repo-URL, Branch) stehen validiert in der
// .git/config. Installation/Deinstallation ueber File-/Directory-APIs.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace IScrapLauncher
{
    internal static class Program
    {
        internal static LauncherForm Form;
        internal static Config Cfg = new Config();

        private static readonly Regex BranchRx = new Regex("^[A-Za-z0-9._/-]{1,64}$");
        private static readonly Regex HttpUrlRx = new Regex("^https://[A-Za-z0-9._~:/?#@!$&'()*+,;=%-]{4,300}$");

        private const string CmdInit =
            "@echo off\r\n" +
            "cd /d \"%~dp0..\\packrepo\"\r\n" +
            "git init\r\n" +
            "exit /b %ERRORLEVEL%\r\n";
        private const string CmdFetch =
            "@echo off\r\n" +
            "cd /d \"%~dp0..\\packrepo\"\r\n" +
            "git fetch origin\r\n" +
            "if errorlevel 1 exit /b 1\r\n" +
            "exit /b 0\r\n";
        private const string CmdReset =
            "@echo off\r\n" +
            "cd /d \"%~dp0..\\packrepo\"\r\n" +
            "git reset --hard FETCH_HEAD\r\n" +
            "exit /b %ERRORLEVEL%\r\n";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            Cfg.Load(Path.Combine(exeDir, "launcher.ini"));

            Form = new LauncherForm();
            bool cli = false;
            foreach (string a in args)
            {
                cli = true;
                string s = a.ToLowerInvariant();
                if (s == "--detect") { Detect(); Console.WriteLine("GAMEPATH=" + Cfg.GamePath); break; }
                if (s == "--install") { Detect(); RunInstall(true); break; }
                if (s == "--update") { Detect(); RunUpdate(); break; }
                if (s == "--launch") { Detect(); LaunchGame(); break; }
                if (s.StartsWith("--path=", StringComparison.Ordinal)) { SetGamePath(a.Substring(7)); break; }
            }
            if (!cli) Application.Run(Form);
        }

        private static void Detect() { if (string.IsNullOrEmpty(Cfg.GamePath)) Cfg.GamePath = FindGame(); }

        internal static void SetGamePath(string p)
        {
            if (LooksLikeGame(p)) { Cfg.GamePath = p; Cfg.Save(); Form.Log("Spiel-Pfad gesetzt: " + p); }
            else Form.Log("Ungueltiger Spiel-Pfad (keine GTAIV.exe): " + p);
        }

        // ------------------------------------------------------------ ShellExecuteEx (P/Invoke)
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ShellExecuteInfo
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            public string lpVerb;
            public string lpFile;
            public string lpParameters;
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShellExecuteEx(ref ShellExecuteInfo info);

        private const uint SeeMaskNocloseprocess = 0x00000040;
        private const int SwHide = 0;

        /// <summary>Fuehrt eine Batch-Datei aus _cmd hidden aus und wartet; Rueckgabe = Exit-Code.
        /// lpFile ist IMMER cmd.exe (normale EXE); batchArgs sind feste Literale wie "/c git_init.cmd".</summary>
        private static int RunCmdBatch(string batchArgs)
        {
            ShellExecuteInfo info = new ShellExecuteInfo();
            info.cbSize = Marshal.SizeOf(typeof(ShellExecuteInfo));
            info.fMask = SeeMaskNocloseprocess;
            info.lpVerb = "open";
            info.lpFile = "cmd.exe";
            info.lpParameters = batchArgs;
            info.lpDirectory = CmdDir();
            info.nShow = SwHide;
            if (!ShellExecuteEx(ref info)) throw new IOException("Start fehlgeschlagen (Win32 " + Marshal.GetLastWin32Error() + "): cmd.exe " + batchArgs);
            if (info.hProcess == IntPtr.Zero) return 0;
            return WaitForHandle(info.hProcess, 600000);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static int WaitForHandle(IntPtr handle, uint timeoutMs)
        {
            WaitForSingleObject(handle, timeoutMs);
            uint code;
            GetExitCodeProcess(handle, out code);
            CloseHandle(handle);
            return unchecked((int)code);
        }

        // ------------------------------------------------------------ Konfiguration
        internal class Config
        {
            public string GamePath = "";
            public string RepoUrl = "https://github.com/daCptn/iscrap-modpack.git";
            public string Branch = "main";

            public void Load(string path)
            {
                try
                {
                    if (!File.Exists(path)) return;
                    foreach (string raw in File.ReadAllLines(path))
                    {
                        string line = raw.Trim();
                        int eq = line.IndexOf('=');
                        if (eq < 1) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();
                        if (k.Equals("GamePath", StringComparison.OrdinalIgnoreCase)) GamePath = v;
                        else if (k.Equals("RepoUrl", StringComparison.OrdinalIgnoreCase)) RepoUrl = v;
                        else if (k.Equals("Branch", StringComparison.OrdinalIgnoreCase)) Branch = v;
                    }
                }
                catch { }
            }

            public void Save()
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("; iScrap Mod-Pack Launcher");
                    sb.AppendLine("GamePath=" + GamePath);
                    sb.AppendLine("RepoUrl=" + RepoUrl);
                    sb.AppendLine("Branch=" + Branch);
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher.ini"), sb.ToString());
                }
                catch { }
            }
        }

        // ------------------------------------------------------------ Spielsuche
        internal static string FindGame()
        {
            List<string> candidates = new List<string>();
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (k != null)
                    {
                        object v = k.GetValue("SteamPath");
                        if (v != null) AddSteamLib(candidates, v.ToString());
                    }
                }
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                AddSteamLib(candidates, Path.Combine(pf, "Steam"));
                foreach (char d in new char[] { 'C', 'D', 'E', 'F', 'G', 'H' })
                    candidates.Add(d + ":\\Steam");
            }
            catch { }

            foreach (string lib in candidates)
            {
                string game = Path.Combine(lib, "steamapps", "common", "Grand Theft Auto IV", "GTAIV");
                if (File.Exists(Path.Combine(game, "GTAIV.exe"))) return game;
            }
            return "";
        }

        private static void AddSteamLib(List<string> libs, string steamRoot)
        {
            if (string.IsNullOrEmpty(steamRoot) || !Directory.Exists(steamRoot)) return;
            libs.Add(steamRoot);
            try
            {
                string vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) return;
                foreach (string raw in File.ReadAllLines(vdf))
                {
                    string line = raw.Trim().Replace("\\\\", "\\");
                    if (!line.StartsWith("\"path\"")) continue;
                    int q1 = line.IndexOf('"', 7);
                    if (q1 < 0) continue;
                    int q2 = line.IndexOf('"', q1 + 1);
                    if (q2 < 0) continue;
                    libs.Add(line.Substring(q1 + 1, q2 - q1 - 1));
                }
            }
            catch { }
        }

        internal static bool LooksLikeGame(string p)
        {
            try
            {
                return !string.IsNullOrEmpty(p) && File.Exists(Path.Combine(p, "GTAIV.exe"));
            }
            catch { return false; }
        }

        // ------------------------------------------------------------ Git (konstante Batch-Dateien)
        private static string RepoDir()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packrepo");
        }

        private static string CmdDir()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_cmd");
        }

        private static string WriteCmd(string name, string content)
        {
            string dir = CmdDir();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
            return path;
        }

        private static bool RepoConfigValid()
        {
            if (!BranchRx.IsMatch(Cfg.Branch) || Cfg.Branch.StartsWith("-"))
            {
                Form.Log("FEHLER: ungueltiger Branch in launcher.ini: " + Cfg.Branch);
                return false;
            }
            bool ok = HttpUrlRx.IsMatch(Cfg.RepoUrl) ||
                      (Path.IsPathRooted(Cfg.RepoUrl) && !Cfg.RepoUrl.Contains("\"") &&
                       !Cfg.RepoUrl.Contains(".."));
            if (!ok) Form.Log("FEHLER: ungueltige RepoUrl in launcher.ini (nur https:// oder lokaler Ordner): " + Cfg.RepoUrl);
            return ok;
        }

        private static void WriteGitConfig(string dir)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[core]");
            sb.AppendLine("\tbare = false");
            sb.AppendLine("[remote \"origin\"]");
            sb.AppendLine("\turl = " + Cfg.RepoUrl);
            sb.AppendLine("\tfetch = +refs/heads/" + Cfg.Branch + ":refs/remotes/origin/" + Cfg.Branch);
            File.WriteAllText(Path.Combine(dir, ".git", "config"), sb.ToString());
        }

        internal static bool EnsureRepo()
        {
            string dir = RepoDir();
            if (!RepoConfigValid()) return false;
            if (!Directory.Exists(Path.Combine(dir, ".git")))
            {
                if (Directory.Exists(dir)) { try { Directory.Delete(dir, true); } catch { } }
                Directory.CreateDirectory(dir);
                Form.Log("Initialisiere Paket-Repo: " + Cfg.RepoUrl);
                WriteCmd("git_init.cmd", CmdInit); if (RunCmdBatch("/c git_init.cmd") != 0) return false;
                WriteGitConfig(dir);
            }
            else
            {
                WriteGitConfig(dir);
                Form.Log("Aktualisiere Paket-Repo ...");
            }
            WriteCmd("git_fetch.cmd", CmdFetch); if (RunCmdBatch("/c git_fetch.cmd") != 0) { Form.Log("git fetch fehlgeschlagen"); return false; }
            WriteCmd("git_reset.cmd", CmdReset); if (RunCmdBatch("/c git_reset.cmd") != 0) { Form.Log("git reset fehlgeschlagen"); return false; }
            return true;
        }

        internal static string ReadVersion(string path)
        {
            try
            {
                if (!File.Exists(path)) return "";
                foreach (string raw in File.ReadAllLines(path))
                {
                    string v = raw.Trim();
                    if (v.Length > 0) return v;
                }
            }
            catch { }
            return "";
        }

        // ------------------------------------------------------------ Deinstallation
        internal static void DoUninstall(string game)
        {
            // Ueberschriebene Original-Spieldateien zuerst restaurieren (Pack liefert .bak mit).
            // handling.dat loeschen statt restaurieren wuerde das Spiel brechen.
            string handling = Path.Combine(game, "common", "data", "handling.dat");
            if (File.Exists(handling + ".bak") && File.Exists(handling))
            {
                try { File.Copy(handling + ".bak", handling, true); Form.Log("  restauriert (Original): common/data/handling.dat"); }
                catch (Exception ex) { Form.Log("  handling.dat-Restore fehlgeschlagen: " + ex.Message); }
            }
            string list = Path.Combine(RepoDir(), "uninstall.txt");
            string keep = Path.Combine(RepoDir(), "keep.txt");
            List<string> keepers = File.Exists(keep)
                ? File.ReadAllLines(keep).Select(l => l.Trim().ToLowerInvariant()).Where(l => l.Length > 0).ToList()
                : new List<string>();
            if (!File.Exists(list)) { Form.Log("uninstall.txt fehlt im Repo — Ueberspringe Deinstallation"); return; }
            int n = 0;
            foreach (string raw in File.ReadAllLines(list))
            {
                string rel = raw.Trim();
                if (rel.Length == 0 || rel.StartsWith("#")) continue;
                string low = rel.ToLowerInvariant().TrimEnd('/', '\\');
                bool guarded = keepers.Any(k => low == k || low.EndsWith("\\" + k) || low.EndsWith("/" + k));
                if (guarded) { Form.Log("  Uebersprungen (geschuetzt): " + rel); continue; }
                string full = Path.Combine(game, rel.Replace('/', Path.DirectorySeparatorChar));
                string fullLow = full.ToLowerInvariant();
                string gameLow = game.ToLowerInvariant().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!fullLow.StartsWith(gameLow)) { Form.Log("  Uebersprungen (ausserhalb): " + rel); continue; }
                try
                {
                    if (File.Exists(full)) { File.Delete(full); n++; Form.Log("  entfernt: " + rel); }
                    else if (Directory.Exists(full)) { Directory.Delete(full, true); n++; Form.Log("  entfernt: " + rel + Path.DirectorySeparatorChar); }
                }
                catch (Exception ex) { Form.Log("  Konnte " + rel + " nicht entfernen: " + ex.Message); }
            }
            Form.Log("Deinstallation: " + n + " Eintraege entfernt (Saves blieben unberuehrt).");
        }

        // ------------------------------------------------------------ Installation
        private const string FusionFixUrl =
            "https://github.com/ThirteenAG/GTAIV.EFLC.FusionFix/releases/download/v5.1.1/GTAIV.EFLC.FusionFix.zip";

        /// <summary>FusionFix (Grossdateien >100 MB) kommt vom offiziellen Release, nicht aus dem Repo.</summary>
        internal static void EnsureFusionFix(string game)
        {
            string marker = Path.Combine(game, "update", "GTAIV.EFLC.FusionFix", "GTAIV.EFLC.FusionFix.img");
            if (File.Exists(marker)) { Form.Log("FusionFix bereits installiert."); return; }
            Form.Log("Lade FusionFix vom offiziellen Release (~204 MB, einmalig) ...");
            string zip = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fusionfix.zip");
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(FusionFixUrl, zip);
                }
                string extract = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ff_extract");
                if (Directory.Exists(extract)) { try { Directory.Delete(extract, true); } catch { } }
                Directory.CreateDirectory(extract);
                ZipFile.ExtractToDirectory(zip, extract);
                CopyTree(extract, game);
                try { Directory.Delete(extract, true); } catch { }
                try { File.Delete(zip); } catch { }
                Form.Log("FusionFix installiert.");
            }
            catch (Exception ex)
            {
                Form.Log("FusionFix-Download/Entpacken fehlgeschlagen: " + ex.Message);
                Form.Log("Manuell nachinstallieren: " + FusionFixUrl);
            }
        }

        // ------------------------------------------------------------ Ruckler-Fix (Registry)
        // Nexus 1513 ("High Priority Registry Tweak") als Standard-IFEO-Werte: CPU-
        // Prioritaet Hoch (3), E/A-Prioritaet Hoch (3), Speicherseiten-Prioritaet Sehr
        // Hoch (5) fuer GTAIV.exe, nativ + WOW6432Node. Die Elevation macht ein eigener
        // Helper (RucklerFix.exe, requireAdministrator-Manifest) — runas auf cmd.exe
        // lieferte in der Praxis Win32 5 (Access Denied).
        internal static bool TweakActive()
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTAIV.exe"))
                {
                    return k != null && k.GetValue("CpuPriorityClass") != null;
                }
            }
            catch { return false; }
        }

        internal static void ToggleTweak()
        {
            string helper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RucklerFix.exe");
            if (!File.Exists(helper))
            {
                Form.Log("RucklerFix.exe fehlt neben dem Launcher — bitte beide EXEs aus dem Repo (launcher/) verwenden.");
                return;
            }
            bool wasActive = TweakActive();
            Form.Log((wasActive ? "Entferne" : "Setze") + " Ruckler-Fix (CPU/E/A/Speicher-Prioritaet fuer GTAIV.exe) ... [UAC-Prompt]");
            try
            {
                ShellExecuteInfo info = new ShellExecuteInfo();
                info.cbSize = Marshal.SizeOf(typeof(ShellExecuteInfo));
                info.fMask = SeeMaskNocloseprocess;
                info.lpVerb = "open";   // Elevation kommt aus dem Helper-Manifest
                info.lpFile = helper;
                info.lpParameters = wasActive ? "remove" : "apply";
                info.nShow = 1;
                if (!ShellExecuteEx(ref info))
                {
                    int err = Marshal.GetLastWin32Error();
                    Form.Log(err == 1223
                        ? "UAC-Prompt abgebrochen — nichts geaendert."
                        : "Ruckler-Fix fehlgeschlagen (Win32 " + err + ").");
                    return;
                }
                if (info.hProcess != IntPtr.Zero)
                {
                    WaitForHandle(info.hProcess, 60000);   // wartet auch, bis der Nutzer den UAC-Prompt bedient
                }
                Form.Log("Ruckler-Fix ist jetzt " + (TweakActive()
                    ? "AN — wirkt ab dem naechsten Spielstart, rueckgaengig ueber denselben Button."
                    : "aus (Vanilla-Prioritaet)."));
            }
            catch (Exception ex) { Form.Log("Ruckler-Fix fehlgeschlagen: " + ex.Message); }
        }

        // ------------------------------------------------------------ Extra-Pakete (packages/)
        // Mods, deren Lizenz Re-Uploads verbietet (LC Customs, First Degree 154), kommen
        // NICHT ins Git-Repo: Der Nutzer legt die Nexus-Archive (.zip/.7z/.rar) in den
        // packages/-Ordner neben dem Launcher. Zip entpackt .NET selbst; 7z/rar gehen
        // ueber das Windows-eigene tar.exe (libarchive) oder ein installiertes 7-Zip —
        // jeweils als generierte Batch-Datei (Pfade stehen im Dateiinhalt, nie in
        // Kommandozeilen-Argumenten). Ueberschriebene Originaldateien landen in
        // packages_backup/<Paketname>/.
        internal static void InstallPackages(string game)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages");
            if (!Directory.Exists(dir))
            {
                try { Directory.CreateDirectory(dir); } catch { }
                Form.Log("packages/-Ordner angelegt: " + dir);
                Form.Log("Archive der Extra-Mods (LC Customs, First Degree 154 — Nexus-Login noetig) dort hineinlegen, dann erneut klicken.");
                return;
            }
            List<string> archives = new List<string>();
            foreach (string f in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext == ".zip" || ext == ".7z" || ext == ".rar") archives.Add(f);
            }
            if (archives.Count == 0) { Form.Log("Keine .zip/.7z/.rar im packages/-Ordner gefunden."); return; }
            string gameLow = game.ToLowerInvariant().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string backupRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages_backup");
            foreach (string archive in archives)
            {
                string name = Path.GetFileNameWithoutExtension(archive);
                string done = Path.Combine(dir, name + ".installed");
                if (File.Exists(done)) { Form.Log("Bereits installiert, uebersprungen: " + name); continue; }
                Form.Log("Installiere Extra-Paket: " + Path.GetFileName(archive));
                string extract = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pkg_extract", name);
                try
                {
                    if (Directory.Exists(extract)) { try { Directory.Delete(extract, true); } catch { } }
                    Directory.CreateDirectory(extract);
                    string ext2 = Path.GetExtension(archive).ToLowerInvariant();
                    if (ext2 == ".zip") ZipFile.ExtractToDirectory(archive, extract);
                    else if (!ExtractArchive(archive, extract))
                    {
                        Form.Log("  Archiv konnte nicht entpackt werden — bitte als .zip ablegen oder 7-Zip installieren.");
                        continue;
                    }
                    int files = 0, backed = 0;
                    foreach (string file in Directory.GetFiles(extract, "*", SearchOption.AllDirectories))
                    {
                        string rel = file.Substring(extract.Length).TrimStart(Path.DirectorySeparatorChar);
                        string target = Path.Combine(game, rel);
                        if (!target.ToLowerInvariant().StartsWith(gameLow))
                        { Form.Log("  uebersprungen (ausserhalb des Spiels): " + rel); continue; }
                        string tdir = Path.GetDirectoryName(target);
                        if (!Directory.Exists(tdir)) Directory.CreateDirectory(tdir);
                        if (File.Exists(target) && IsProtectedIni(tdir, Path.GetFileName(target)))
                        { Form.Log("  uebersprungen (geschuetzt): " + rel); continue; }
                        if (File.Exists(target))
                        {
                            string bfile = Path.Combine(backupRoot, name, rel);
                            string bdir = Path.GetDirectoryName(bfile);
                            if (!Directory.Exists(bdir)) Directory.CreateDirectory(bdir);
                            if (!File.Exists(bfile)) File.Copy(target, bfile, false);
                            backed++;
                        }
                        File.Copy(file, target, true);
                        files++;
                    }
                    File.WriteAllText(done, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
                    Form.Log("  " + files + " Dateien installiert, " + backed + " Originaldateien nach packages_backup/" + name + "/ gesichert.");
                    try { Directory.Delete(extract, true); } catch { }
                }
                catch (Exception ex) { Form.Log("  PAKET FEHLGESCHLAGEN: " + ex.Message); }
            }
        }

        /// <summary>Entpackt .7z/.rar per Windows-tar.exe (libarchive liest beide) oder,
        /// falls tar fehlt, per installiertem 7-Zip. Rueckgabe = Erfolg.</summary>
        private static bool ExtractArchive(string archive, string destDir)
        {
            string tool = Path.Combine(Environment.SystemDirectory, "tar.exe");
            if (!File.Exists(tool))
            {
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                tool = Path.Combine(pf, "7-Zip", "7z.exe");
            }
            if (!File.Exists(tool))
            {
                Form.Log("  Weder C:\\Windows\\System32\\tar.exe noch 7-Zip gefunden.");
                return false;
            }
            string toolArgs = tool.EndsWith("7z.exe", StringComparison.OrdinalIgnoreCase)
                ? "x \"" + archive + "\" -o\"" + destDir + "\" -y"
                : "-xf \"" + archive + "\" -C \"" + destDir + "\"";
            StringBuilder bat = new StringBuilder();
            bat.Append("@echo off\r\n");
            bat.Append("if not exist \"" + destDir + "\" mkdir \"" + destDir + "\"\r\n");
            bat.Append("\"" + tool + "\" " + toolArgs + "\r\n");
            bat.Append("exit /b %ERRORLEVEL%\r\n");
            WriteCmd("pkg_extract.cmd", bat.ToString());
            return RunCmdBatch("/c pkg_extract.cmd") == 0;
        }

        internal static void DoInstall(string game)
        {
            string src = Path.Combine(RepoDir(), "modpack");
            if (!Directory.Exists(src)) { Form.Log("FEHLER: modpack/ fehlt im Repo"); return; }
            CopyTree(src, game);
            EnsureFusionFix(game);
            string ver = ReadVersion(Path.Combine(RepoDir(), "version.txt"));
            File.WriteAllText(Path.Combine(game, "scripts", "pack_version.txt"), ver + Environment.NewLine);
            Form.Log("Installation abgeschlossen. Version: " + ver);
            Form.Log("Tipp: Ruckler-Fix (Registry) und Extra-Pakete sind eigene Buttons oben.");
        }

        private static void CopyTree(string src, string dst)
        {
            foreach (string dir in Directory.GetDirectories(src))
            {
                string name = Path.GetFileName(dir);
                string target = Path.Combine(dst, name);
                if (!Directory.Exists(target)) Directory.CreateDirectory(target);
                CopyTree(dir, target);
            }
            foreach (string file in Directory.GetFiles(src))
            {
                string name = Path.GetFileName(file);
                string target = Path.Combine(dst, name);
                if (File.Exists(target) && IsProtectedIni(dst, name)) { Form.Log("  uebersprungen (vorhanden): " + name); continue; }
                File.Copy(file, target, true);
            }
        }

        private static bool IsProtectedIni(string dst, string name)
        {
            if (!name.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) return false;
            if (!Path.GetFileName(dst.TrimEnd(Path.DirectorySeparatorChar)).Equals("scripts", StringComparison.OrdinalIgnoreCase)) return false;
            return name.Equals("PhonePlus.ini", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("PhonePlusMarkers.ini", StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------ Aktionen
        internal static void RunInstall(bool withUninstall)
        {
            if (!LooksLikeGame(Cfg.GamePath)) { Form.Log("Spiel-Pfad ungueltig — erst Pfad waehlen."); return; }
            Form.Log("=== Saubere Installation ===");
            if (!EnsureRepo()) return;
            if (withUninstall) DoUninstall(Cfg.GamePath);
            DoInstall(Cfg.GamePath);
        }

        internal static void RunUpdate()
        {
            if (!LooksLikeGame(Cfg.GamePath)) { Form.Log("Spiel-Pfad ungueltig — erst Pfad waehlen."); return; }
            Form.Log("=== Update-Pruefung ===");
            if (!EnsureRepo()) return;
            string repoVer = ReadVersion(Path.Combine(RepoDir(), "version.txt"));
            string instPath = Path.Combine(Cfg.GamePath, "scripts", "pack_version.txt");
            string instVer = File.Exists(instPath) ? ReadVersion(instPath) : "";
            Form.Log("Installiert: " + (instVer.Length == 0 ? "(nichts)" : instVer) + "  |  Repo: " + repoVer);
            if (repoVer.Length == 0) { Form.Log("Repo enthaelt keine version.txt — Abbruch."); return; }
            if (repoVer == instVer) { Form.Log("Alles aktuell. Nichts zu tun."); return; }
            Form.Log("Update verfuegbar — saubere Neuinstallation ...");
            DoUninstall(Cfg.GamePath);
            DoInstall(Cfg.GamePath);
        }

        internal static void LaunchGame()
        {
            if (!LooksLikeGame(Cfg.GamePath)) { Form.Log("Spiel-Pfad ungueltig — erst Pfad waehlen."); return; }
            string[] candidates = { "PlayGTAIV.exe", "LaunchGTAIV.exe", "GTAIV.exe" };
            foreach (string c in candidates)
            {
                string full = Path.Combine(Cfg.GamePath, c);
                if (File.Exists(full))
                {
                    try
                    {
                        ShellExecuteInfo info = new ShellExecuteInfo();
                        info.cbSize = Marshal.SizeOf(typeof(ShellExecuteInfo));
                        info.fMask = SeeMaskNocloseprocess;
                        info.lpVerb = "open";
                        info.lpFile = full;
                        info.lpDirectory = Cfg.GamePath;
                        info.nShow = 1; // SW_SHOWNORMAL
                        if (ShellExecuteEx(ref info))
                        {
                            Form.Log("Spiel gestartet: " + c);
                            if (info.hProcess != IntPtr.Zero) CloseHandle(info.hProcess);
                            return;
                        }
                        Form.Log("Start fehlgeschlagen (" + c + "): ShellExecuteEx-Fehler " + Marshal.GetLastWin32Error());
                    }
                    catch (Exception ex) { Form.Log("Start fehlgeschlagen (" + c + "): " + ex.Message); }
                }
            }
            Form.Log("Kein Spiel-EXE gefunden.");
        }
    }

    internal class LauncherForm : Form
    {
        private TextBox log;
        private Label pathLabel;

        public LauncherForm()
        {
            Text = "iScrap Mod-Pack Launcher — GTA IV Complete Edition";
            Width = 780; Height = 520; FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;

            pathLabel = new Label { Left = 12, Top = 12, Width = 620, Height = 20 };
            Button changePath = new Button { Text = "Pfad...", Left = 660, Top = 10, Width = 90 };
            changePath.Click += delegate
            {
                using (FolderBrowserDialog d = new FolderBrowserDialog())
                {
                    d.Description = "GTA IV-Ordner auswaehlen (enthaelt GTAIV.exe)";
                    if (d.ShowDialog(this) != DialogResult.OK) return;
                    if (Program.LooksLikeGame(d.SelectedPath))
                    {
                        Program.SetGamePath(d.SelectedPath);
                        UpdatePath();
                    }
                    else
                    {
                        MessageBox.Show(this, "Hier liegt keine GTAIV.exe.", "Pfad ungueltig");
                    }
                }
            };

            Button update = new Button { Text = "Update pruefen & installieren", Left = 12, Top = 44, Width = 240, Height = 40 };
            update.Click += delegate { Background(Program.RunUpdate); };
            Button install = new Button { Text = "Pack installieren / reparieren", Left = 264, Top = 44, Width = 240, Height = 40 };
            install.Click += delegate { Background(delegate { Program.RunInstall(true); }); };
            Button launch = new Button { Text = "Spiel starten", Left = 516, Top = 44, Width = 234, Height = 40 };
            launch.Click += delegate { Program.LaunchGame(); };

            Button tweak = new Button { Text = "Ruckler-Fix (Registry): ?", Left = 12, Top = 92, Width = 240, Height = 34 };
            tweak.Text = "Ruckler-Fix (Registry): " + (Program.TweakActive() ? "AN" : "aus");
            // Synchron im UI-Thread: reg.exe dauert Sekunden und der Button-Text wird danach sicher aktualisiert.
            tweak.Click += delegate { Program.ToggleTweak(); tweak.Text = "Ruckler-Fix (Registry): " + (Program.TweakActive() ? "AN" : "aus"); };
            Button packages = new Button { Text = "Extra-Pakete installieren (packages/)", Left = 264, Top = 92, Width = 240, Height = 34 };
            packages.Click += delegate { Background(delegate { Program.InstallPackages(Program.Cfg.GamePath); }); };
            Label hint = new Label
            {
                Text = "Extra-Mods (Lizenz! nicht im Repo): Nexus-Archive\r\n(zip/7z/rar) in den packages-Ordner neben dem\r\nLauncher legen, dann diesen Button druecken.",
                Left = 516, Top = 88, Width = 240, Height = 40,
                Font = new System.Drawing.Font("Segoe UI", 7.5f), ForeColor = System.Drawing.Color.DimGray
            };

            log = new TextBox { Left = 12, Top = 134, Width = 738, Height = 322, Multiline = true,
                ScrollBars = ScrollBars.Vertical, ReadOnly = true, BackColor = System.Drawing.Color.FromArgb(12, 16, 14),
                ForeColor = System.Drawing.Color.FromArgb(140, 220, 160), Font = new System.Drawing.Font("Consolas", 9f) };

            Controls.Add(pathLabel); Controls.Add(changePath);
            Controls.Add(update); Controls.Add(install); Controls.Add(launch);
            Controls.Add(tweak); Controls.Add(packages); Controls.Add(hint);
            Controls.Add(log);
            UpdatePath();

            if (!Program.LooksLikeGame(Program.Cfg.GamePath))
            {
                string found = Program.FindGame();
                if (Program.LooksLikeGame(found)) { Program.Cfg.GamePath = found; Program.Cfg.Save(); UpdatePath(); }
            }
            Log("Launcher bereit. Spiel: " + (Program.LooksLikeGame(Program.Cfg.GamePath) ? Program.Cfg.GamePath : "(nicht gefunden — Pfad waehlen)"));
        }

        private void UpdatePath()
        {
            pathLabel.Text = "Spiel: " + (Program.Cfg.GamePath.Length > 0 ? Program.Cfg.GamePath : "(nicht gesetzt)");
        }

        private void Background(Action a)
        {
            foreach (Control c in Controls) if (c is Button) c.Enabled = false;
            new System.Threading.Thread(delegate()
            {
                try { a(); } catch (Exception ex) { Log("FEHLER: " + ex.Message); }
                try { BeginInvoke((Action)delegate { foreach (Control c in Controls) if (c is Button) c.Enabled = true; }); } catch { }
            }) { IsBackground = true }.Start();
        }

        internal void Log(string s)
        {
            try
            {
                if (log.InvokeRequired) { log.BeginInvoke((Action)delegate { Log(s); }); return; }
                log.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + s + Environment.NewLine);
            }
            catch { }
        }
    }
}
