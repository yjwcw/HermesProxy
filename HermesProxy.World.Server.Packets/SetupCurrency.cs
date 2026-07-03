using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SetupCurrency : ServerPacket
{
	public struct Record
	{
		public uint Type;

		public uint Quantity;

		public uint? WeeklyQuantity;

		public uint? MaxWeeklyQuantity;

		public uint? TrackedQuantity;

		public int? MaxQuantity;

		public int? Unused901;

		public byte Flags;
	}

	public List<Record> Data = new List<Record>();

	public SetupCurrency()
		: base(Opcode.SMSG_SETUP_CURRENCY, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Data.Count);
		foreach (Record datum in Data)
		{
			_worldPacket.WriteUInt32(datum.Type);
			_worldPacket.WriteUInt32(datum.Quantity);
			_worldPacket.WriteBit(datum.WeeklyQuantity.HasValue);
			_worldPacket.WriteBit(datum.MaxWeeklyQuantity.HasValue);
			_worldPacket.WriteBit(datum.TrackedQuantity.HasValue);
			_worldPacket.WriteBit(datum.MaxQuantity.HasValue);
			_worldPacket.WriteBit(datum.Unused901.HasValue);
			_worldPacket.WriteBits(datum.Flags, 5);
			_worldPacket.FlushBits();
			if (datum.WeeklyQuantity.HasValue)
			{
				_worldPacket.WriteUInt32(datum.WeeklyQuantity.Value);
			}
			if (datum.MaxWeeklyQuantity.HasValue)
			{
				_worldPacket.WriteUInt32(datum.MaxWeeklyQuantity.Value);
			}
			if (datum.TrackedQuantity.HasValue)
			{
				_worldPacket.WriteUInt32(datum.TrackedQuantity.Value);
			}
			if (datum.MaxQuantity.HasValue)
			{
				_worldPacket.WriteInt32(datum.MaxQuantity.Value);
			}
			if (datum.Unused901.HasValue)
			{
				_worldPacket.WriteInt32(datum.Unused901.Value);
			}
		}
	}
}
