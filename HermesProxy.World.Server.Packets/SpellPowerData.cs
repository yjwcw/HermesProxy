using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public struct SpellPowerData
{
	public int Cost;

	public PowerType Type;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(Cost);
		data.WriteInt8((sbyte)Type);
	}
}
