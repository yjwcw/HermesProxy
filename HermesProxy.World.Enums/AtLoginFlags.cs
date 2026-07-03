namespace HermesProxy.World.Enums;

public enum AtLoginFlags
{
	None = 0,
	Rename = 1,
	ResetSpells = 2,
	ResetTalents = 4,
	Customize = 8,
	ResetPetTalents = 0x10,
	FirstLogin = 0x20,
	ChangeFaction = 0x40,
	ChangeRace = 0x80,
	Resurrect = 0x100
}
