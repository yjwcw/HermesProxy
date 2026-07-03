using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums.Classic;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct InventorySlots
{
	public const byte BagStart = 19;

	public const byte BagEnd = 23;

	public const byte ItemStart = 23;

	public const byte ItemEnd = 47;

	public const byte BankItemStart = 47;

	public const byte BankItemEnd = 75;

	public const byte BankBagStart = 75;

	public const byte BankBagEnd = 82;

	public const byte BuyBackStart = 82;

	public const byte BuyBackEnd = 94;

	public const byte KeyringStart = 94;

	public const byte KeyringEnd = 126;

	public const byte Bag0 = byte.MaxValue;

	public const byte DefaultSize = 16;
}
