using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum CastFlag : uint
{
	None = 0u,
	PendingCast = 1u,
	HasTrajectory = 2u,
	Unknown2 = 4u,
	Unknown3 = 8u,
	Unknown4 = 0x10u,
	Projectile = 0x20u,
	Unknown5 = 0x40u,
	Unknown6 = 0x80u,
	Unknown7 = 0x100u,
	Unknown8 = 0x200u,
	Unknown9 = 0x400u,
	PredictedPower = 0x800u,
	Unknown10 = 0x1000u,
	Unknown11 = 0x2000u,
	Unknown12 = 0x4000u,
	Unknown13 = 0x8000u,
	Unknown14 = 0x10000u,
	AdjustMissile = 0x20000u,
	NoGcd = 0x40000u,
	VisualChain = 0x80000u,
	Unknown18 = 0x100000u,
	RuneInfo = 0x200000u,
	Unknown19 = 0x400000u,
	Unknown20 = 0x800000u,
	Unknown21 = 0x1000000u,
	Unknown22 = 0x2000000u,
	Immunity = 0x4000000u,
	Unknown23 = 0x8000000u,
	Unknown24 = 0x10000000u,
	Unknown25 = 0x20000000u,
	HealPrediction = 0x40000000u,
	Unknown27 = 0x80000000u
}
