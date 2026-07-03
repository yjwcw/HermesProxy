namespace HermesProxy.World.Enums;

public enum ArenaTeamCommandErrorModern : uint
{
	None = 0u,
	Internal = 1u,
	AlreadyInArenaTeam = 2u,
	AlreadyInArenaTeamS = 3u,
	InvitedToArenaTeam = 4u,
	AlreadyInvitedToArenaTeamS = 5u,
	NameInvalid = 6u,
	NameExistsS = 7u,
	LeaderLeaveS = 8u,
	Permissions = 8u,
	PlayerNotInTeam = 9u,
	PlayerNotInTeamSS = 10u,
	PlayerNotFoundS = 11u,
	NotALlied = 12u,
	IgnoringYouS = 19u,
	Internal2 = 20u,
	TargetTooLowS = 21u,
	TargetTooHighS = 22u,
	TooManyMembersS = 23u,
	NotFound = 27u,
	Locked = 30u,
	TooManyCreate = 33u,
	Disqualified = 42u
}
