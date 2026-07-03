namespace HermesProxy.World.Enums;

public enum PlayerLocalFlags
{
	ControllingPet = 1,
	TrackStealthed = 2,
	ReleaseTimer = 8,
	NoReleaseWindow = 0x10,
	NoPetBar = 0x20,
	OverrideCameraMinHeight = 0x40,
	NewlyBosstedCharacter = 0x80,
	UsingPartGarrison = 0x100,
	CanUseObjectsMounted = 0x200,
	CanVisitPartyGarrison = 0x400,
	WarMode = 0x800,
	AccountSecured = 0x1000,
	OverrideTransportServerTime = 0x8000,
	MentorRestricted = 0x20000,
	WeeklyRewardAvailable = 0x40000
}
