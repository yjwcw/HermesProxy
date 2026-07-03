namespace HermesProxy.World.Enums;

internal enum PartyResultModern : byte
{
	Ok = 0,
	BadPlayerName = 1,
	TargetNotInGroup = 2,
	TargetNotInInstance = 3,
	GroupFull = 4,
	AlreadyInGroup = 5,
	NotInGroup = 6,
	NotLeader = 7,
	PlayerWrongFaction = 8,
	IgnoringYou = 9,
	LfgPending = 12,
	InviteRestricted = 13,
	GroupSwapFailed = 14,
	InviteUnknownRealm = 15,
	InviteNoPartyServer = 16,
	InvitePartyBusy = 17,
	PartyTargetAmbiguous = 18,
	PartyLfgInviteRaidLocked = 19,
	PartyLfgBootLimit = 20,
	PartyLfgBootCooldown = 21,
	PartyLfgBootInProgress = 22,
	PartyLfgBootTooFewPlayers = 23,
	PartyLfgBootNotEligible = 24,
	RaidDisallowedByLevel = 25,
	PartyLfgBootInCombat = 26,
	VoteKickReasonNeeded = 27,
	PartyLfgBootDungeonComplete = 28,
	PartyLfgBootLootRolls = 29,
	PartyLfgTeleportInCombat = 30
}
