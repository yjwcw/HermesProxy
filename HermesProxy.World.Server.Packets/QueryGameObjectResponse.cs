using Framework.Constants;
using Framework.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryGameObjectResponse : ServerPacket
{
	public uint GameObjectID;

	public WowGuid128 Guid;

	public bool Allow;

	public GameObjectStats Stats;

	public QueryGameObjectResponse()
		: base(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(GameObjectID);
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteBit(Allow);
		_worldPacket.FlushBits();
		ByteBuffer byteBuffer = new ByteBuffer();
		if (Allow)
		{
			byteBuffer.WriteUInt32(Stats.Type);
			byteBuffer.WriteUInt32(Stats.DisplayID);
			for (int i = 0; i < 4; i++)
			{
				byteBuffer.WriteCString(Stats.Name[i]);
			}
			byteBuffer.WriteCString(Stats.IconName);
			byteBuffer.WriteCString(Stats.CastBarCaption);
			byteBuffer.WriteCString(Stats.UnkString);
			int num = (ModernVersion.AddedInClassicVersion(1, 14, 1, 2, 5, 3) ? 35 : 34);
			for (int j = 0; j < num; j++)
			{
				byteBuffer.WriteInt32(Stats.Data[j]);
			}
			byteBuffer.WriteFloat(Stats.Size);
			byteBuffer.WriteUInt8((byte)Stats.QuestItems.Count);
			foreach (uint questItem in Stats.QuestItems)
			{
				byteBuffer.WriteUInt32(questItem);
			}
			byteBuffer.WriteUInt32(Stats.ContentTuningId);
		}
		_worldPacket.WriteUInt32(byteBuffer.GetSize());
		if (byteBuffer.GetSize() != 0)
		{
			_worldPacket.WriteBytes(byteBuffer);
		}
	}
}
