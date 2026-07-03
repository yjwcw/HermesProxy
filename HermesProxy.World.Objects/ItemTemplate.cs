using HermesProxy.Enums;

namespace HermesProxy.World.Objects;

public class ItemTemplate
{
	public uint Entry;

	public int Class;

	public uint SubClass;

	public int SoundOverrideSubclass;

	public string[] Name = new string[4];

	public uint DisplayID;

	public int Quality;

	public uint Flags;

	public uint FlagsExtra;

	public uint BuyCount;

	public uint BuyPrice;

	public uint SellPrice;

	public int InventoryType;

	public int AllowedClasses;

	public int AllowedRaces;

	public uint ItemLevel;

	public uint RequiredLevel;

	public uint RequiredSkillId;

	public uint RequiredSkillLevel;

	public uint RequiredSpell;

	public uint RequiredHonorRank;

	public uint RequiredCityRank;

	public uint RequiredRepFaction;

	public uint RequiredRepValue;

	public int MaxCount;

	public int MaxStackSize;

	public uint ContainerSlots;

	public uint StatsCount;

	public int[] StatTypes = new int[10];

	public int[] StatValues = new int[10];

	public int ScalingStatDistribution;

	public uint ScalingStatValue;

	public float[] DamageMins = new float[5];

	public float[] DamageMaxs = new float[5];

	public int[] DamageTypes = new int[5];

	public uint Armor;

	public uint HolyResistance;

	public uint FireResistance;

	public uint NatureResistance;

	public uint FrostResistance;

	public uint ShadowResistance;

	public uint ArcaneResistance;

	public uint Delay;

	public int AmmoType;

	public float RangedMod;

	public int[] TriggeredSpellIds = new int[5];

	public int[] TriggeredSpellTypes = new int[5];

	public int[] TriggeredSpellCharges = new int[5];

	public int[] TriggeredSpellCooldowns = new int[5];

	public uint[] TriggeredSpellCategories = new uint[5];

	public int[] TriggeredSpellCategoryCooldowns = new int[5];

	public int Bonding;

	public string Description;

	public uint PageText;

	public int Language;

	public int PageMaterial;

	public uint StartQuestId;

	public uint LockId;

	public int Material;

	public int SheathType;

	public int RandomProperty;

	public uint RandomSuffix;

	public uint Block;

	public uint ItemSet;

	public uint MaxDurability;

	public uint AreaID;

	public int MapID;

	public uint BagFamily;

	public int TotemCategory;

	public int[] ItemSocketColors = new int[3];

	public uint[] SocketContent = new uint[3];

	public int SocketBonus;

	public int GemProperties;

	public int RequiredDisenchantSkill;

	public float ArmorDamageModifier;

	public uint Duration;

	public int ItemLimitCategory;

	public int HolidayID;

	public void ReadFromLegacyPacket(uint entry, WorldPacket packet)
	{
		Entry = entry;
		Class = packet.ReadInt32();
		SubClass = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_3_6299))
		{
			SoundOverrideSubclass = packet.ReadInt32();
		}
		for (int i = 0; i < 4; i++)
		{
			Name[i] = packet.ReadCString();
		}
		DisplayID = packet.ReadUInt32();
		Quality = packet.ReadInt32();
		Flags = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			FlagsExtra = packet.ReadUInt32();
		}
		BuyPrice = packet.ReadUInt32();
		SellPrice = packet.ReadUInt32();
		InventoryType = packet.ReadInt32();
		AllowedClasses = packet.ReadInt32();
		AllowedRaces = packet.ReadInt32();
		ItemLevel = packet.ReadUInt32();
		RequiredLevel = packet.ReadUInt32();
		RequiredSkillId = packet.ReadUInt32();
		RequiredSkillLevel = packet.ReadUInt32();
		RequiredSpell = packet.ReadUInt32();
		RequiredHonorRank = packet.ReadUInt32();
		RequiredCityRank = packet.ReadUInt32();
		RequiredRepFaction = packet.ReadUInt32();
		RequiredRepValue = packet.ReadUInt32();
		MaxCount = packet.ReadInt32();
		MaxStackSize = packet.ReadInt32();
		ContainerSlots = packet.ReadUInt32();
		StatsCount = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? packet.ReadUInt32() : 10u);
		if (StatsCount > 10)
		{
			StatTypes = new int[StatsCount];
			StatValues = new int[StatsCount];
		}
		for (int j = 0; j < StatsCount; j++)
		{
			StatTypes[j] = packet.ReadInt32();
			StatValues[j] = packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			ScalingStatDistribution = packet.ReadInt32();
			ScalingStatValue = packet.ReadUInt32();
		}
		int num = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767) ? 2 : 5);
		for (int k = 0; k < num; k++)
		{
			DamageMins[k] = packet.ReadFloat();
			DamageMaxs[k] = packet.ReadFloat();
			DamageTypes[k] = packet.ReadInt32();
		}
		Armor = packet.ReadUInt32();
		HolyResistance = packet.ReadUInt32();
		FireResistance = packet.ReadUInt32();
		NatureResistance = packet.ReadUInt32();
		FrostResistance = packet.ReadUInt32();
		ShadowResistance = packet.ReadUInt32();
		ArcaneResistance = packet.ReadUInt32();
		Delay = packet.ReadUInt32();
		AmmoType = packet.ReadInt32();
		RangedMod = packet.ReadFloat();
		for (byte b = 0; b < 5; b++)
		{
			TriggeredSpellIds[b] = packet.ReadInt32();
			TriggeredSpellTypes[b] = packet.ReadInt32();
			TriggeredSpellCharges[b] = packet.ReadInt32();
			TriggeredSpellCooldowns[b] = packet.ReadInt32();
			TriggeredSpellCategories[b] = packet.ReadUInt32();
			TriggeredSpellCategoryCooldowns[b] = packet.ReadInt32();
			if (TriggeredSpellIds[b] != 0)
			{
				GameData.SaveItemEffectSlot(Entry, (uint)TriggeredSpellIds[b], b);
			}
		}
		Bonding = packet.ReadInt32();
		Description = packet.ReadCString();
		PageText = packet.ReadUInt32();
		Language = packet.ReadInt32();
		PageMaterial = packet.ReadInt32();
		StartQuestId = packet.ReadUInt32();
		LockId = packet.ReadUInt32();
		Material = packet.ReadInt32();
		if (Material < 0)
		{
			Material = 0;
		}
		SheathType = packet.ReadInt32();
		RandomProperty = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			RandomSuffix = packet.ReadUInt32();
		}
		Block = packet.ReadUInt32();
		ItemSet = packet.ReadUInt32();
		MaxDurability = packet.ReadUInt32();
		AreaID = packet.ReadUInt32();
		MapID = packet.ReadInt32();
		BagFamily = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			TotemCategory = packet.ReadInt32();
			for (int l = 0; l < 3; l++)
			{
				ItemSocketColors[l] = packet.ReadInt32();
				SocketContent[l] = packet.ReadUInt32();
			}
			SocketBonus = packet.ReadInt32();
			GemProperties = packet.ReadInt32();
			RequiredDisenchantSkill = packet.ReadInt32();
			ArmorDamageModifier = packet.ReadFloat();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_2_8209))
		{
			Duration = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			ItemLimitCategory = packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			HolidayID = packet.ReadInt32();
		}
	}
}
