using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

public class QuestObjective
{
	public static uint QuestObjectiveCounter = 1u;

	public uint Id;

	public uint QuestID;

	public QuestObjectiveType Type;

	public sbyte StorageIndex;

	public int ObjectID;

	public int Amount;

	public QuestObjectiveFlags Flags;

	public uint Flags2;

	public float ProgressBarWeight;

	public string Description;

	public int[] VisualEffects = Array.Empty<int>();

	public bool IsStoringValue()
	{
		switch (Type)
		{
		case QuestObjectiveType.Monster:
		case QuestObjectiveType.Item:
		case QuestObjectiveType.GameObject:
		case QuestObjectiveType.TalkTo:
		case QuestObjectiveType.PlayerKills:
		case QuestObjectiveType.WinPvpPetBattles:
		case QuestObjectiveType.HaveCurrency:
		case QuestObjectiveType.ObtainCurrency:
		case QuestObjectiveType.IncreaseReputation:
			return true;
		default:
			return false;
		}
	}

	public bool IsStoringFlag()
	{
		switch (Type)
		{
		case QuestObjectiveType.AreaTrigger:
		case QuestObjectiveType.WinPetBattleAgainstNpc:
		case QuestObjectiveType.DefeatBattlePet:
		case QuestObjectiveType.CriteriaTree:
		case QuestObjectiveType.AreaTriggerEnter:
		case QuestObjectiveType.AreaTriggerExit:
			return true;
		default:
			return false;
		}
	}

	public static bool CanAlwaysBeProgressedInRaid(QuestObjectiveType type)
	{
		switch (type)
		{
		case QuestObjectiveType.Item:
		case QuestObjectiveType.Currency:
		case QuestObjectiveType.LearnSpell:
		case QuestObjectiveType.MinReputation:
		case QuestObjectiveType.MaxReputation:
		case QuestObjectiveType.Money:
		case QuestObjectiveType.HaveCurrency:
		case QuestObjectiveType.IncreaseReputation:
			return true;
		default:
			return false;
		}
	}
}
