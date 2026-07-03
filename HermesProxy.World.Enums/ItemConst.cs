using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ItemConst
{
	public const int MaxDamages = 2;

	public const int MaxGemSockets = 3;

	public const int MaxSpells = 5;

	public const int MaxStats = 10;

	public const int MaxBagSize = 36;

	public const byte NullBag = 0;

	public const byte NullSlot = byte.MaxValue;

	public const int MaxOutfitItems = 24;

	public const int MaxItemExtCostItems = 5;

	public const int MaxItemExtCostCurrencies = 5;

	public const int MaxItemEnchantmentEffects = 3;

	public const int MaxProtoSpells = 5;

	public const int MaxEquipmentSetIndex = 20;

	public const int MaxItemSubclassTotal = 21;

	public const int MaxItemSetItems = 17;

	public const int MaxItemSetSpells = 8;

	public static uint[] ItemQualityColors = new uint[8] { 4288519581u, 4294967295u, 4280221440u, 4278218973u, 4288886254u, 4294934528u, 4293315712u, 4293315712u };
}
