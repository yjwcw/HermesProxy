namespace HermesProxy.World.Server.Packets;

public class RequestPlayedTime : ClientPacket
{
	public bool TriggerScriptEvent;

	public RequestPlayedTime(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TriggerScriptEvent = _worldPacket.HasBit();
	}
}
