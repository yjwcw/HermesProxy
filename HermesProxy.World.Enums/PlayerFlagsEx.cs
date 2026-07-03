using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum PlayerFlagsEx
{
	ReagentBankUnlocked = 1,
	MercenaryMode = 2,
	ArtifactForgeCheat = 4,
	InPvpCombat = 0x40,
	HideHelm = 0x80,
	HideCloak = 0x100,
	UnlockedAoeLoot = 0x200
}
