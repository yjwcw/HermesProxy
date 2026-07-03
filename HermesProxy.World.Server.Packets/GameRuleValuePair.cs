namespace HermesProxy.World.Server.Packets;

public class GameRuleValuePair
{
	public int Rule;

	public int Value;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(Rule);
		data.WriteInt32(Value);
	}
}
