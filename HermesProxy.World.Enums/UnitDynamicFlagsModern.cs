using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum UnitDynamicFlagsModern : uint
{
	None = 0u,
	HideModel = 2u,
	Lootable = 4u,
	TrackUnit = 8u,
	Tapped = 0x10u,
	EmpathyInfo = 0x20u,
	AppearDead = 0x40u,
	ReferAFriendLinked = 0x80u
}
