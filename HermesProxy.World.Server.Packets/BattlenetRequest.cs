namespace HermesProxy.World.Server.Packets;

internal class BattlenetRequest : ClientPacket
{
	public MethodCall Method;

	public byte[] Data;

	public BattlenetRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Method.Read(_worldPacket);
		uint count = _worldPacket.ReadUInt32();
		Data = _worldPacket.ReadBytes(count);
	}
}
