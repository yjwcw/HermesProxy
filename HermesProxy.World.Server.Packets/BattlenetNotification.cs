using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class BattlenetNotification : ServerPacket
{
	public MethodCall Method;

	public ByteBuffer Data = new ByteBuffer();

	public BattlenetNotification()
		: base(Opcode.SMSG_BATTLENET_NOTIFICATION)
	{
	}

	public override void Write()
	{
		Method.Write(_worldPacket);
		_worldPacket.WriteUInt32(Data.GetSize());
		_worldPacket.WriteBytes(Data);
	}
}
