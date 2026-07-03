using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum PlayerExtraFlags
{
	GMOn = 1,
	AcceptWhispers = 4,
	TaxiCheat = 8,
	GMInvisible = 0x10,
	GMChat = 0x20,
	PVPDeath = 0x100,
	HasRaceChanged = 0x200,
	GrantedLevelsFromRaf = 0x400,
	LevelBoosted = 0x800
}
