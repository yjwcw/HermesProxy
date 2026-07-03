using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class RuneData
{
	public byte Start;

	public byte Count;

	public List<byte> Cooldowns = new List<byte>();

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(Start);
		data.WriteUInt8(Count);
		data.WriteInt32(Cooldowns.Count);
		foreach (byte cooldown in Cooldowns)
		{
			data.WriteUInt8(cooldown);
		}
	}
}
