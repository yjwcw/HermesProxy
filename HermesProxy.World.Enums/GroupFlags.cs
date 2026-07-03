using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupFlags
{
	None = 0,
	FakeRaid = 1,
	Raid = 2,
	LfgRestricted = 4,
	Lfg = 8,
	Destroyed = 0x10,
	OnePersonParty = 0x20,
	EveryoneAssistant = 0x40,
	GuildGroup = 0x100,
	MaskBgRaid = 3
}
