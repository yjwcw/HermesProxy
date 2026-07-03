using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EquipmentSlot
{
	public const byte Start = 0;

	public const byte Head = 0;

	public const byte Neck = 1;

	public const byte Shoulders = 2;

	public const byte Shirt = 3;

	public const byte Chest = 4;

	public const byte Waist = 5;

	public const byte Legs = 6;

	public const byte Feet = 7;

	public const byte Wrist = 8;

	public const byte Hands = 9;

	public const byte Finger1 = 10;

	public const byte Finger2 = 11;

	public const byte Trinket1 = 12;

	public const byte Trinket2 = 13;

	public const byte Cloak = 14;

	public const byte MainHand = 15;

	public const byte OffHand = 16;

	public const byte Ranged = 17;

	public const byte Tabard = 18;

	public const byte End = 19;

	public const byte Bag1 = 19;

	public const byte Bag2 = 20;

	public const byte Bag3 = 21;

	public const byte Bag4 = 22;
}
