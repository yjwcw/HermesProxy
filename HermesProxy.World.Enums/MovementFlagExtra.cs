using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum MovementFlagExtra : ushort
{
	None = 0,
	PreventStrafe = 1,
	PreventJumping = 2,
	DisableCollision = 4,
	FullSpeedTurning = 8,
	FullSpeedPitching = 0x10,
	AlwaysAllowPitching = 0x20,
	IsVehicleExitVoluntary = 0x40,
	IsJumpSplineInAir = 0x80,
	IsAnimTierInTrans = 0x100,
	PreventChangePitch = 0x200,
	InterpolateMove = 0x400,
	InterpolateTurning = 0x800,
	InterpolatePitching = 0x1000,
	VehiclePassengerIsTransitionAllowed = 0x2000,
	CanTransitionBetweenSwimAndFly = 0x4000,
	Unknown10 = 0x8000
}
