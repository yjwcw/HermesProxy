namespace HermesProxy.World.Enums;

public enum PetitionTurnResult
{
	Ok = 0,
	AlreadyInGuild = 2,
	NeedMoreSignatures = 4,
	GuildPermissions = 11,
	GuildNameInvalid = 12,
	HasRestriction = 13
}
