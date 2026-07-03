using System;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class QueryPetNameResponse : ServerPacket
{
	public WowGuid128 UnitGUID;

	public bool Allow;

	public bool HasDeclined;

	public DeclinedName DeclinedNames = new DeclinedName();

	public long Timestamp;

	public string Name = "";

	public QueryPetNameResponse()
		: base(Opcode.SMSG_QUERY_PET_NAME_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(UnitGUID);
		_worldPacket.WriteBit(Allow);
		if (Allow)
		{
			_worldPacket.WriteBits(Name.GetByteCount(), 8);
			_worldPacket.WriteBit(HasDeclined);
			for (byte b = 0; b < 5; b++)
			{
				_worldPacket.WriteBits(DeclinedNames.name[b].GetByteCount(), 7);
			}
			for (byte b2 = 0; b2 < 5; b2++)
			{
				_worldPacket.WriteString(DeclinedNames.name[b2]);
			}
			_worldPacket.WriteInt64(Timestamp);
			_worldPacket.WriteString(Name);
		}
		_worldPacket.FlushBits();
	}
}
