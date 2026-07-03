using System;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class GossipPOI : ServerPacket
{
	public uint Id = 1u;

	public uint Flags;

	public Vector3 Pos;

	public uint Icon;

	public uint Importance;

	public uint Unknown905;

	public string Name;

	public GossipPOI()
		: base(Opcode.SMSG_GOSSIP_POI)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Id);
		_worldPacket.WriteFloat(Pos.X);
		_worldPacket.WriteFloat(Pos.Y);
		_worldPacket.WriteFloat(Pos.Z);
		_worldPacket.WriteUInt32(Icon);
		_worldPacket.WriteUInt32(Importance);
		_worldPacket.WriteUInt32(Unknown905);
		_worldPacket.WriteBits(Flags, 14);
		_worldPacket.WriteBits(Name.GetByteCount(), 6);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(Name);
	}
}
