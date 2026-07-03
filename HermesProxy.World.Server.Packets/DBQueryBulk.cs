using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class DBQueryBulk : ClientPacket
{
	public DB2Hash TableHash;

	public List<uint> Queries = new List<uint>();

	public DBQueryBulk(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TableHash = (DB2Hash)_worldPacket.ReadUInt32();
		uint num = _worldPacket.ReadBits<uint>(13);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Queries.Add(_worldPacket.ReadUInt32());
		}
	}
}
