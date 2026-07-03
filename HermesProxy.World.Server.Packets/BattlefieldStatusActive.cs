using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BattlefieldStatusActive : ServerPacket
{
	public BattlefieldStatusHeader Hdr = new BattlefieldStatusHeader();

	public uint Mapid;

	public uint ShutdownTimer;

	public uint StartTimer;

	public byte ArenaFaction;

	public bool LeftEarly;

	public BattlefieldStatusActive()
		: base(Opcode.SMSG_BATTLEFIELD_STATUS_ACTIVE)
	{
	}

	public override void Write()
	{
		Hdr.Write(_worldPacket);
		_worldPacket.WriteUInt32(Mapid);
		_worldPacket.WriteUInt32(ShutdownTimer);
		_worldPacket.WriteUInt32(StartTimer);
		_worldPacket.WriteBit(ArenaFaction != 0);
		_worldPacket.WriteBit(LeftEarly);
		_worldPacket.FlushBits();
	}
}
