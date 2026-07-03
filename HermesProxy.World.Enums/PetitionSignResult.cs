namespace HermesProxy.World.Enums;

public enum PetitionSignResult
{
	Ok = 0,
	AlreadySigned = 1,
	AlreadyInGuild = 2,
	CantSignOwn = 3,
	NotServer = 5,
	Full = 8,
	AlreadySignedOther = 10,
	RestrictedAccountTrial = 11,
	HasRestriction = 13
}
