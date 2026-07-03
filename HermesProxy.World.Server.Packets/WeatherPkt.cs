using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class WeatherPkt : ServerPacket
{
	public bool Abrupt;

	public float Intensity;

	public WeatherState WeatherID;

	public WeatherPkt()
		: base(Opcode.SMSG_WEATHER, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)WeatherID);
		_worldPacket.WriteFloat(Intensity);
		_worldPacket.WriteBit(Abrupt);
		_worldPacket.FlushBits();
	}
}
