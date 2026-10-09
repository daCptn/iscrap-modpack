// Ruckler-Fix-Helper — setzt/entfernt die IFEO-Prioritaetswerte fuer GTAIV.exe.
// Eigenes EXE mit requireAdministrator-Manifest: Die UAC-Elevation kommt aus dem
// Manifest (ShellExecuteEx "open"), weil runas auf cmd.exe je nach Umgebung mit
// Win32 5 (Access Denied) scheitert. Schreibt direkt ueber die Registry-API.
// Aufruf: RucklerFix.exe apply | remove   — Exit-Code 0 = Erfolg.
using System;
using Microsoft.Win32;

static class RucklerFix
{
    private static readonly string[] IfeoKeys = {
        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTAIV.exe",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTAIV.exe"
    };

    private static int Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "";
        if (mode != "apply" && mode != "remove") return 2;
        try
        {
            foreach (string key in IfeoKeys)
            {
                using (RegistryKey k = Registry.LocalMachine.CreateSubKey(key))
                {
                    if (k == null) return 3;
                    if (mode == "apply")
                    {
                        // CPU-Prioritaet Hoch, E/A-Prioritaet Hoch, Speicherseiten-Prioritaet Sehr Hoch
                        k.SetValue("CpuPriorityClass", 3, RegistryValueKind.DWord);
                        k.SetValue("IoPriority", 3, RegistryValueKind.DWord);
                        k.SetValue("PagePriority", 5, RegistryValueKind.DWord);
                    }
                    else
                    {
                        foreach (string v in new string[] { "CpuPriorityClass", "IoPriority", "PagePriority" })
                        {
                            try { k.DeleteValue(v, false); } catch { }
                        }
                    }
                }
            }
            return 0;
        }
        catch { return 1; }
    }
}
