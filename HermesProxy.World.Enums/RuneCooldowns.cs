using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RuneCooldowns
{
	public const int Base = 10000;

	public const int Miss = 1500;
}
