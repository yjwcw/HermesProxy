using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum SplineFlagWotLK : uint
{
	None = 0u,
	AnimTierSwim = 1u,
	AnimTierHover = 2u,
	AnimTierFly = 3u,
	AnimTierSubmerged = 4u,
	Done = 0x100u,
	Falling = 0x200u,
	NoSpline = 0x400u,
	Trajectory = 0x800u,
	WalkMode = 0x1000u,
	Flying = 0x2000u,
	Knockback = 0x4000u,
	FinalPoint = 0x8000u,
	FinalTarget = 0x10000u,
	FinalOrientation = 0x20000u,
	CatmullRom = 0x40000u,
	Cyclic = 0x80000u,
	EnterCycle = 0x100000u,
	AnimationTier = 0x200000u,
	Frozen = 0x400000u,
	Transport = 0x800000u,
	TransportExit = 0x1000000u,
	Unknown7 = 0x2000000u,
	Unknown8 = 0x4000000u,
	OrientationInverted = 0x8000000u,
	UsePathSmoothing = 0x10000000u,
	Animation = 0x20000000u,
	UncompressedPath = 0x40000000u,
	Unknown10 = 0x80000000u
}
