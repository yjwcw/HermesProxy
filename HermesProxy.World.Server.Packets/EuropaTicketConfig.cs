namespace HermesProxy.World.Server.Packets;

public class EuropaTicketConfig
{
	public bool TicketsEnabled;

	public bool BugsEnabled;

	public bool ComplaintsEnabled;

	public bool SuggestionsEnabled;

	public SavedThrottleObjectState ThrottleState;

	public void Write(WorldPacket data)
	{
		data.WriteBit(TicketsEnabled);
		data.WriteBit(BugsEnabled);
		data.WriteBit(ComplaintsEnabled);
		data.WriteBit(SuggestionsEnabled);
		ThrottleState.Write(data);
	}
}
