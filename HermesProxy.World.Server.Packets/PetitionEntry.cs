namespace HermesProxy.World.Server.Packets;

public struct PetitionEntry
{
	public uint Index;

	public uint CharterCost;

	public uint CharterEntry;

	public uint IsArena;

	public uint RequiredSignatures;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Index);
		data.WriteUInt32(CharterCost);
		data.WriteUInt32(CharterEntry);
		data.WriteUInt32(IsArena);
		data.WriteUInt32(RequiredSignatures);
	}
}
