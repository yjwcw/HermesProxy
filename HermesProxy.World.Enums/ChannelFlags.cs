using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum ChannelFlags : uint
{
	None = 0u,
	AutoJoin = 1u,
	ZoneBased = 2u,
	ReadOnly = 4u,
	AllowItemLinks = 8u,
	OnlyInCities = 0x10u,
	LinkedChannel = 0x20u,
	ZoneAttackAlerts = 0x10000u,
	GuildRecruitment = 0x20000u
}
