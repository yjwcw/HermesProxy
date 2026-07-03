using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum MovementFlagWotLK : uint
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
	OnTransport = 0x200u,
	DisableGravity = 0x400u,
	Root = 0x800u,
	Falling = 0x1000u,
	FallingFar = 0x2000u,
	PendingStop = 0x4000u,
	PendingStrafeStop = 0x8000u,
	PendingForward = 0x10000u,
	PendingBackward = 0x20000u,
	PendingStrafeLeft = 0x40000u,
	PendingStrafeRight = 0x80000u,
	PendingRoot = 0x100000u,
	Swimming = 0x200000u,
	Ascending = 0x400000u,
	Descending = 0x800000u,
	CanFly = 0x1000000u,
	Flying = 0x2000000u,
	SplineElevation = 0x4000000u,
	SplineEnabled = 0x8000000u,
	Waterwalking = 0x10000000u,
	CanSafeFall = 0x20000000u,
	Hover = 0x40000000u,
	LocalDirty = 0x80000000u
}
