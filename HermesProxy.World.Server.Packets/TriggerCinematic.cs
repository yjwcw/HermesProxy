using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TriggerCinematic : ServerPacket
{
	public uint CinematicID;

	public TriggerCinematic()
		: base(Opcode.SMSG_TRIGGER_CINEMATIC)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(CinematicID);
	}
}
