using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Objects;

public class PlayerData
{
	public WowGuid128 DuelArbiter;

	public WowGuid128 WowAccount;

	public WowGuid128 LootTargetGUID;

	public uint? PlayerFlags;

	public uint? PlayerFlagsEx;

	public uint? GuildRankID;

	public uint? GuildDeleteDate;

	public int? GuildLevel;

	public byte? PartyType;

	public byte? NumBankSlots;

	public byte? NativeSex;

	public byte? Inebriation;

	public byte? PvpTitle;

	public byte? ArenaFaction;

	public byte? PvPRank;

	public uint? DuelTeam;

	public int? GuildTimeStamp;

	public QuestLog[] QuestLog = new QuestLog[25];

	public VisibleItem[] VisibleItems = new VisibleItem[19];

	public int? ChosenTitle;

	public int? FakeInebriation;

	public uint? VirtualPlayerRealm;

	public uint? CurrentSpecID;

	public int? TaxiMountAnimKitID;

	public uint? CurrentBattlePetBreedQuality;

	public int? HonorLevel;

	public ChrCustomizationChoice[] Customizations = new ChrCustomizationChoice[36];

	public float?[] AvgItemLevel { get; } = new float?[6];

}
