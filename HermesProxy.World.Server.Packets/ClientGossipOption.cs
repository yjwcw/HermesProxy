using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ClientGossipOption
{
	public int OptionIndex;

	public byte OptionIcon;

	public byte OptionFlags;

	public int OptionCost;

	public uint Language;

	public GossipOptionStatus Status;

	public string Text;

	public string Confirm;

	public TreasureLootList Treasure = new TreasureLootList();

	public int? SpellID;
}
