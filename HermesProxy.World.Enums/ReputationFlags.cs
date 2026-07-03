using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum ReputationFlags : ushort
{
	None = 0,
	Visible = 1,
	AtWar = 2,
	Hidden = 4,
	Header = 8,
	Peaceful = 0x10,
	Inactive = 0x20,
	ShowPropagated = 0x40,
	HeaderShowsBar = 0x80,
	CapitalCityForRaceChange = 0x100,
	Guild = 0x200,
	GarrisonInvasion = 0x400
}
