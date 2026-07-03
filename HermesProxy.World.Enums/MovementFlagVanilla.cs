using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum MovementFlagVanilla : uint
{
	None = 0u,
	Forward = 1u,
	Backward = 2u,
	StrafeLeft = 4u,
	StrafeRight = 8u,
	TurnLeft = 0x10u,
	TurnRight = 0x20u,
	PitchUp = 0x40u,
	PitchDown = 0x80u,
	WalkMode = 0x100u,
	OnTransport = 0x2000000u,
	Levitating = 0x400u,
	FixedZ = 0x800u,
	Root = 0x1000u,
	Falling = 0x2000u,
	FallingFar = 0x4000u,
	Swimming = 0x200000u,
	SplineEnabled = 0x400000u,
	CanFly = 0x800000u,
	Flying = 0x1000000u,
	SplineElevation = 0x4000000u,
	Waterwalking = 0x10000000u,
	CanSafeFall = 0x20000000u,
	Hover = 0x40000000u
}
