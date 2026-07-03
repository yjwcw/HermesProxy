namespace HermesProxy.World.Server.Packets;

public struct SpellExtraCurrencyCost
{
	public int CurrencyID;

	public int Slot;

	public int Count;

	public void Read(WorldPacket data)
	{
		CurrencyID = data.ReadInt32();
		Slot = data.ReadInt32();
		Count = data.ReadInt32();
	}
}
