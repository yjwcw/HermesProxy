using System;
using System.Runtime.InteropServices;

namespace HermesProxy;

internal static class OsSpecific
{
	public static bool AreWeInOurOwnConsole()
	{
		try
		{
			GetWindowThreadProcessId(GetConsoleWindow(), out var lpdwProcessId);
			return lpdwProcessId == Environment.ProcessId;
		}
		catch
		{
			return false;
		}
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
