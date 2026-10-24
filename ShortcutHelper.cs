using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace WeboIconSpike;

/// <summary>
/// Creates or updates .lnk shortcuts (test locations 5 and 6) through the classic IShellLink COM API.
/// Rule D3: it only ever touches shortcuts that carry its own marker in the description;
/// a shortcut created by the user is never modified.
/// </summary>
public static class ShortcutHelper
{
    private const string Marker = "WeboIconSpike-managed";

    public static string Upsert(string lnkPath, string targetExe, string iconPath)
    {
        IShellLinkW? link = null;
        try
        {
            link = (IShellLinkW)new CShellLink();
            var file = (IPersistFile)link;
            var existed = File.Exists(lnkPath);

            if (existed)
            {
                file.Load(lnkPath, 0);
                var sb = new StringBuilder(1024);
                link.GetDescription(sb, sb.Capacity);
                if (!sb.ToString().Contains(Marker))
                    return $"REFUSED: \"{lnkPath}\" was not created by this spike, so it was left untouched";
            }

            link.SetPath(targetExe);
            link.SetWorkingDirectory(Path.GetDirectoryName(targetExe) ?? "");
            link.SetDescription("Webo icon spike shortcut " + Marker);
            link.SetIconLocation(iconPath, 0);

            Directory.CreateDirectory(Path.GetDirectoryName(lnkPath)!);
            file.Save(lnkPath, true);
            return $"API OK: {(existed ? "updated" : "created")} \"{lnkPath}\" with icon \"{iconPath}\" (visual result NOT verified)";
        }
        catch (Exception ex)
        {
            return $"FAIL: shortcut operation threw {ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            if (link != null) Marshal.ReleaseComObject(link);
        }
    }

    /// <summary>Asks Explorer to refresh associations/icons (SHCNE_ASSOCCHANGED). Used to test whether shortcut icons refresh without a restart.</summary>
    public static string NotifyShell()
    {
        try
        {
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
            return "API OK: SHChangeNotify(SHCNE_ASSOCCHANGED) sent";
        }
        catch (Exception ex)
        {
            return $"FAIL: SHChangeNotify threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
    private class CShellLink { }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}
