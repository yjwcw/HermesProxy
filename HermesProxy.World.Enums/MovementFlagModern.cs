using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum MovementFlagModern : uint
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
	DisableGravity = 0x200u,
	Root = 0x400u,
	Falling = 0x800u,
	FallingFar = 0x1000u,
	PendingStop = 0x2000u,
	PendingStrafeStop = 0x4000u,
	PendingForward = 0x8000u,
	PendingBackward = 0x10000u,
	PendingStrafeLeft = 0x20000u,
	PendingStrafeRight = 0x40000u,
	PendingRoot = 0x80000u,
	Swimming = 0x100000u,
	Ascending = 0x200000u,
	Descending = 0x400000u,
	CanFly = 0x800000u,
	Flying = 0x1000000u,
	SplineElevation = 0x2000000u,
	Waterwalking = 0x4000000u,
	CanSafeFall = 0x8000000u,
	Hover = 0x10000000u,
	DisableCollision = 0x20000000u,
	MaskMoving = 0x60080Fu
}
