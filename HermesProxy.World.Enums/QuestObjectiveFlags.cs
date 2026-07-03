namespace HermesProxy.World.Enums;

public enum QuestObjectiveFlags
{
	TrackedOnMinimap = 1,
	Sequenced = 2,
	Optional = 4,
	Hidden = 8,
	HideCreditMsg = 0x10,
	PreserveQuestItems = 0x20,
	PartOfProgressBar = 0x40,
	KillPlayersSameFaction = 0x80
}
