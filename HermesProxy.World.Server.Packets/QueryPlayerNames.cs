using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class QueryPlayerNames : ClientPacket
{
	public List<WowGuid128> Players = new List<WowGuid128>();

	public QueryPlayerNames(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint num = _worldPacket.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Players.Add(_worldPacket.ReadPackedGuid128());
		}
	}
}
