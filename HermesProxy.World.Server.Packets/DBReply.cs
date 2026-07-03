using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DBReply : ServerPacket
{
	public DB2Hash TableHash;

	public uint Timestamp;

	public uint RecordID;

	public HotfixStatus Status = HotfixStatus.Invalid;

	public ByteBuffer Data = new ByteBuffer();

	public DBReply()
		: base(Opcode.SMSG_DB_REPLY)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)TableHash);
		_worldPacket.WriteUInt32(RecordID);
		_worldPacket.WriteUInt32(Timestamp);
		_worldPacket.WriteBits((byte)Status, 3);
		_worldPacket.WriteUInt32(Data.GetSize());
		_worldPacket.WriteBytes(Data.GetData());
	}
}
