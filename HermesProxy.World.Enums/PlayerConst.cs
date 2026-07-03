using System;
using System.Runtime.InteropServices;

namespace HermesProxy.World.Enums;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PlayerConst
{
	public const int MaxTalentTiers = 7;

	public const int MaxTalentColumns = 3;

	public const int MaxTalentRank = 5;

	public const int MaxPvpTalentSlots = 4;

	public const int MinSpecializationLevel = 10;

	public const int MaxSpecializations = 5;

	public const int InitialSpecializationIndex = 4;

	public const int MaxMasterySpells = 2;

	public const int MaxDeclinedNameCases = 5;

	public const int ReqPrimaryTreeTalents = 31;

	public const int ExploredZonesSize = 192;

	public const ulong MaxMoneyAmount = 99999999999uL;

	public const int MaxActionButtons = 132;

	public const int MaxActionButtonActionValue = 16777216;

	public const int MaxDailyQuests = 25;

	public const int QuestsCompletedBitsSize = 1750;

	public static TimeSpan InfinityCooldownDelay = TimeSpan.FromSeconds(2592000.0);

	public const uint infinityCooldownDelayCheck = 1296000u;

	public const int MaxPlayerSummonDelay = 120;

	public const int TaxiMaskSize = 339;

	public const int DeathExpireStep = 300;

	public const int MaxDeathCount = 3;

	public const int MaxCUFProfiles = 5;

	public static uint[] copseReclaimDelay = new uint[3] { 30u, 60u, 120u };

	public const int MaxRunes = 7;

	public const int MaxRechargingRunes = 3;

	public const int CustomDisplaySize = 3;

	public const int ArtifactsAllWeaponsGeneralWeaponEquippedPassive = 197886;

	public const int MaxArtifactTier = 1;

	public const int MaxHonorLevel = 500;

	public const byte LevelMinHonor = 20;

	public const uint SpellPvpRulesEnabled = 134735u;

	public const uint ItemIdHeartOfAzeroth = 158075u;

	public const uint MaxAzeriteItemLevel = 129u;

	public const uint MaxAzeriteItemKnowledgeLevel = 30u;

	public const uint PlayerConditionIdUnlockedAzeriteEssences = 69048u;

	public const uint SpellIdHeartEssenceActionBarOverride = 298554u;
}
