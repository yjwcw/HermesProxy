using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum SplineFlagModern : uint
{
	None = 0u,
	AnimTierSwim = 1u,
	AnimTierHover = 2u,
	AnimTierFly = 3u,
	AnimTierSubmerged = 4u,
	Unknown0 = 8u,
	FallingSlow = 0x10u,
	Done = 0x20u,
	Falling = 0x40u,
	NoSpline = 0x80u,
	Unknown1 = 0x100u,
	Flying = 0x200u,
	OrientationFixed = 0x400u,
	CatmullRom = 0x800u,
	Cyclic = 0x1000u,
	EnterCycle = 0x2000u,
	Frozen = 0x4000u,
	TransportEnter = 0x8000u,
	TransportExit = 0x10000u,
	Unknown2 = 0x20000u,
	Unknown3 = 0x40000u,
	Backward = 0x80000u,
	SmoothGroundPath = 0x100000u,
	CanSwim = 0x200000u,
	UncompressedPath = 0x400000u,
	Unknown4 = 0x800000u,
	Unknown5 = 0x1000000u,
	Animation = 0x2000000u,
	Parabolic = 0x4000000u,
	FadeObject = 0x8000000u,
	Steering = 0x10000000u,
	Unknown8 = 0x20000000u,
	Unknown9 = 0x40000000u,
	Unknown10 = 0x80000000u
}
