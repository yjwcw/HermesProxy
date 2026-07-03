namespace HermesProxy.World.Server.Packets;

public class SetDungeonDifficulty : ClientPacket
{
	public uint DifficultyID;

	public SetDungeonDifficulty(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		DifficultyID = _worldPacket.ReadUInt32();
	}
}
