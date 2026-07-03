using Framework.Constants;

namespace HermesProxy.World.Server.Packets;

internal class ConnectToFailed : ClientPacket
{
	public ConnectToSerial Serial;

	private byte Con;

	public ConnectToFailed(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Serial = (ConnectToSerial)_worldPacket.ReadUInt32();
		Con = _worldPacket.ReadUInt8();
	}
}
