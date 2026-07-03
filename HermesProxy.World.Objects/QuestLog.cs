namespace HermesProxy.World.Objects;

public class QuestLog
{
	public int? QuestID;

	public uint? StateFlags;

	public uint? EndTime;

	public uint? AcceptTime;

	public short?[] ObjectiveProgress { get; } = new short?[24];

}
