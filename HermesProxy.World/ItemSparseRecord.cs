namespace HermesProxy.World;

public class ItemSparseRecord
{
	public int Id;

	public long AllowableRace;

	public string Description;

	public string Name4;

	public string Name3;

	public string Name2;

	public string Name1;

	public float DmgVariance = 1f;

	public uint DurationInInventory;

	public float QualityModifier;

	public uint BagFamily;

	public float RangeMod;

	public float[] StatPercentageOfSocket = new float[10];

	public int[] StatPercentEditor = new int[10];

	public int Stackable;

	public int MaxCount;

	public uint RequiredAbility;

	public uint SellPrice;

	public uint BuyPrice;

	public uint VendorStackCount = 1u;

	public float PriceVariance = 1f;

	public float PriceRandomValue = 1f;

	public uint[] Flags = new uint[4];

	public int OppositeFactionItemId;

	public uint MaxDurability;

	public ushort ItemNameDescriptionId;

	public ushort RequiredTransmogHoliday;

	public ushort RequiredHoliday;

	public ushort LimitCategory;

	public ushort GemProperties;

	public ushort SocketMatchEnchantmentId;

	public ushort TotemCategoryId;

	public ushort InstanceBound;

	public ushort[] ZoneBound = new ushort[2];

	public ushort ItemSet;

	public ushort LockId;

	public ushort StartQuestId;

	public ushort PageText;

	public ushort Delay;

	public ushort RequiredReputationId;

	public ushort RequiredSkillRank;

	public ushort RequiredSkill;

	public ushort ItemLevel;

	public short AllowableClass;

	public ushort ItemRandomSuffixGroupId;

	public ushort RandomProperty;

	public ushort[] MinDamage = new ushort[5];

	public ushort[] MaxDamage = new ushort[5];

	public short[] Resistances = new short[7];

	public ushort ScalingStatDistributionId;

	public byte ExpansionId = 254;

	public byte ArtifactId;

	public byte SpellWeight;

	public byte SpellWeightCategory;

	public byte[] SocketType = new byte[3];

	public byte SheatheType;

	public byte Material;

	public byte PageMaterial;

	public byte PageLanguage;

	public byte Bonding;

	public byte DamageType;

	public sbyte[] StatType = new sbyte[10];

	public byte ContainerSlots;

	public byte RequiredReputationRank;

	public byte RequiredCityRank;

	public byte RequiredHonorRank;

	public byte InventoryType;

	public byte OverallQualityId;

	public byte AmmoType;

	public sbyte[] StatValue = new sbyte[10];

	public sbyte RequiredLevel;
}
