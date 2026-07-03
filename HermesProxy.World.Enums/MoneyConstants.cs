using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MoneyConstants
{
	public const int Copper = 1;

	public const int Silver = 100;

	public const int Gold = 10000;
}
