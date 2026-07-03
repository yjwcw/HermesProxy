using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum UnitDynamicFlagsLegacy : uint
{
	None = 0u,
	Lootable = 1u,
	TrackUnit = 2u,
	Tapped = 4u,
	TappedByPlayer = 8u,
	EmpathyInfo = 0x10u,
	AppearDead = 0x20u,
	ReferAFriendLinked = 0x40u
}
