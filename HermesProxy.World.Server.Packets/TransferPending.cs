using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TransferPending : ServerPacket
{
	public class ShipTransferPending
	{
		public uint Id;

		public int OriginMapID;
	}

	public uint MapID;

	public Vector3 OldMapPosition;

	public ShipTransferPending Ship;

	public int? TransferSpellID;

	public TransferPending()
		: base(Opcode.SMSG_TRANSFER_PENDING)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteVector3(OldMapPosition);
		_worldPacket.WriteBit(Ship != null);
		_worldPacket.WriteBit(TransferSpellID.HasValue);
		if (Ship != null)
		{
			_worldPacket.WriteUInt32(Ship.Id);
			_worldPacket.WriteInt32(Ship.OriginMapID);
		}
		if (TransferSpellID.HasValue)
		{
			_worldPacket.WriteInt32(TransferSpellID.Value);
		}
		_worldPacket.FlushBits();
	}
}
