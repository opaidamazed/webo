using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Webo.App;

public enum ShortcutResult { Ok, Refused, Failed }

/// <summary>
/// Creates or updates .lnk shortcuts through IShellLink. Decision D3: it only touches shortcuts that carry
/// its own marker; a shortcut made by the user or the installer is never modified.
/// </summary>
public static class ShortcutHelper
{
    private const string Marker = "Webo-managed-shortcut";

    public static ShortcutResult Upsert(string lnkPath, string targetExe, string iconPath, out string error)
    {
        error = "";
        IShellLinkW? link = null;
        try
        {
            link = (IShellLinkW)new CShellLink();
            var file = (IPersistFile)link;

            if (File.Exists(lnkPath))
            {
                file.Load(lnkPath, 0);
                var sb = new StringBuilder(1024);
                link.GetDescription(sb, sb.Capacity);
                if (!sb.ToString().Contains(Marker)) return ShortcutResult.Refused;
            }

            link.SetPath(targetExe);
            link.SetWorkingDirectory(Path.GetDirectoryName(targetExe) ?? "");
            link.SetDescription("Webo " + Marker);
            link.SetIconLocation(iconPath, 0);

            Directory.CreateDirectory(Path.GetDirectoryName(lnkPath)!);
            file.Save(lnkPath, true);
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero); // SHCNE_ASSOCCHANGED
            return ShortcutResult.Ok;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            AppLog.Write("shortcut failed: " + ex);
            return ShortcutResult.Failed;
        }
        finally
        {
            if (link != null) Marshal.ReleaseComObject(link);
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
