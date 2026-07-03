using System.Collections.Generic;

namespace HermesProxy.World.Objects;

public class QuestTemplate
{
	public uint QuestID;

	public int QuestType;

	public int QuestLevel;

	public int QuestScalingFactionGroup;

	public int QuestMaxScalingLevel;

	public uint QuestPackageID;

	public int MinLevel;

	public int QuestSortID;

	public uint QuestInfoID;

	public uint SuggestedGroupNum;

	public uint RewardNextQuest;

	public uint RewardXPDifficulty;

	public float RewardXPMultiplier = 1f;

	public int RewardMoney;

	public uint RewardMoneyDifficulty;

	public float RewardMoneyMultiplier = 1f;

	public uint RewardBonusMoney;

	public uint[] RewardDisplaySpell = new uint[3];

	public uint RewardSpell;

	public uint RewardHonor;

	public float RewardKillHonor;

	public int RewardArtifactXPDifficulty;

	public float RewardArtifactXPMultiplier;

	public int RewardArtifactCategoryID;

	public uint StartItem;

	public uint Flags;

	public uint FlagsEx;

	public uint FlagsEx2;

	public uint POIContinent;

	public float POIx;

	public float POIy;

	public uint POIPriority;

	public long AllowableRaces = -1L;

	public string LogTitle;

	public string LogDescription;

	public string QuestDescription;

	public string AreaDescription;

	public uint RewardTitle;

	public int RewardArenaPoints;

	public uint RewardSkillLineID;

	public uint RewardNumSkillUps;

	public uint PortraitGiver;

	public uint PortraitGiverMount;

	public uint PortraitTurnIn;

	public string PortraitGiverText;

	public string PortraitGiverName;

	public string PortraitTurnInText;

	public string PortraitTurnInName;

	public string QuestCompletionLog;

	public uint RewardFactionFlags;

	public uint AcceptedSoundKitID;

	public uint CompleteSoundKitID;

	public uint AreaGroupID;

	public uint TimeAllowed;

	public int TreasurePickerID;

	public int Expansion;

	public List<QuestObjective> Objectives = new List<QuestObjective>();

	public uint[] RewardItems = new uint[4];

	public uint[] RewardAmount = new uint[4];

	public int[] ItemDrop = new int[4];

	public int[] ItemDropQuantity = new int[4];

	public QuestInfoChoiceItem[] UnfilteredChoiceItems = new QuestInfoChoiceItem[6];

	public uint[] RewardFactionID = new uint[5];

	public int[] RewardFactionValue = new int[5];

	public int[] RewardFactionOverride = new int[5];

	public int[] RewardFactionCapIn = new int[5];

	public uint[] RewardCurrencyID = new uint[4];

	public uint[] RewardCurrencyQty = new uint[4];

	public bool ReadyForTranslation;

	public QuestTemplate()
	{
		LogTitle = "";
		LogDescription = "";
		QuestDescription = "";
		AreaDescription = "";
		PortraitGiverText = "";
		PortraitGiverName = "";
		PortraitTurnInText = "";
		PortraitTurnInName = "";
		QuestCompletionLog = "";
	}
}
