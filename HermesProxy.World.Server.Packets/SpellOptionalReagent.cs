namespace HermesProxy.World.Server.Packets;

public struct SpellOptionalReagent
{
	public int ItemID;

	public int Slot;

	public int Count;

	public void Read(WorldPacket data)
	{
		ItemID = data.ReadInt32();
		Slot = data.ReadInt32();
		Count = data.ReadInt32();
	}
}
