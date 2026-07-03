namespace HermesProxy.World.Server.Packets;

public struct CreatureImmunities
{
	public uint School;

	public uint Value;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(School);
		data.WriteUInt32(Value);
	}
}
