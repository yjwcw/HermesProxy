using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Objects;

public class CorpseData
{
	public WowGuid128 Owner;

	public WowGuid128 PartyGUID;

	public WowGuid128 GuildGUID;

	public uint? DisplayID;

	public byte? RaceId;

	public byte? SexId;

	public byte? ClassId;

	public uint? Flags;

	public uint? DynamicFlags;

	public int? FactionTemplate;

	public ChrCustomizationChoice[] Customizations = new ChrCustomizationChoice[36];

	public uint?[] Items { get; } = new uint?[19];

}
