using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class GuildBankItemInfo
{
	public ItemInstance Item = new ItemInstance();

	public int Slot;

	public int Count;

	public int EnchantmentID;

	public int Charges;

	public int OnUseEnchantmentID;

	public uint Flags;

	public bool Locked;

	public List<ItemGemData> SocketEnchant = new List<ItemGemData>();
}
