using System.Runtime.InteropServices;

namespace Gloam.Console.Render.Platform;

internal static partial class WinVT
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
    private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
    private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr h, out uint mode);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr h, uint mode);

    public static void EnableVT()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var hout = GetStdHandle(STD_OUTPUT_HANDLE);
        if (GetConsoleMode(hout, out uint outMode))
        {
            outMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            SetConsoleMode(hout, outMode);
        }

        var hin = GetStdHandle(STD_INPUT_HANDLE);
        if (GetConsoleMode(hin, out uint inMode))
        {
            inMode &= ~ENABLE_QUICK_EDIT_MODE;
            inMode |= ENABLE_EXTENDED_FLAGS | ENABLE_VIRTUAL_TERMINAL_INPUT;
            SetConsoleMode(hin, inMode);
        }
    }
}
