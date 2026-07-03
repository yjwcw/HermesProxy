using System;

namespace HermesProxy.World.Enums;

[Flags]
internal enum UpdateFlag
{
	None = 0,
	Self = 1,
	Transport = 2,
	AttackingTarget = 4,
	LowGuid = 8,
	HighGuid = 0x10,
	Living = 0x20,
	StationaryObject = 0x40,
	Vehicle = 0x80,
	GOPosition = 0x100,
	GORotation = 0x200,
	Unknown2 = 0x400,
	AnimKits = 0x800,
	TransportUnkArray = 0x1000,
	EnablePortals = 0x2000,
	Unknown = 0x4000
}
