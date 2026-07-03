namespace HermesProxy.World.Server.Packets;

public class QueryCreature : ClientPacket
{
	public uint CreatureID;

	public QueryCreature(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CreatureID = _worldPacket.ReadUInt32();
	}
}
