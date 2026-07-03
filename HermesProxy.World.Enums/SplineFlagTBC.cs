using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum SplineFlagTBC : uint
{
	None = 0u,
	Done = 1u,
	Falling = 2u,
	Unknown3 = 4u,
	Unknown4 = 8u,
	Unknown5 = 0x10u,
	Unknown6 = 0x20u,
	Unknown7 = 0x40u,
	Unknown8 = 0x80u,
	Runmode = 0x100u,
	Flying = 0x200u,
	NoSpline = 0x400u,
	Unknown12 = 0x800u,
	Unknown13 = 0x1000u,
	Unknown14 = 0x2000u,
	Unknown15 = 0x4000u,
	Unknown16 = 0x8000u,
	FinalPoint = 0x10000u,
	FinalTarget = 0x20000u,
	FinalOrientation = 0x40000u,
	Unknown19 = 0x80000u,
	Cyclic = 0x100000u,
	EnterCycle = 0x200000u,
	Frozen = 0x400000u,
	Unknown23 = 0x800000u,
	Unknown24 = 0x1000000u,
	Unknown25 = 0x2000000u,
	Unknown26 = 0x4000000u,
	Unknown27 = 0x8000000u,
	Unknown28 = 0x10000000u,
	Unknown29 = 0x20000000u,
	Unknown30 = 0x40000000u,
	Unknown31 = 0x80000000u
}
