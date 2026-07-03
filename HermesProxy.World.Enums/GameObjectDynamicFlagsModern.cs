using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GameObjectDynamicFlagsModern : uint
{
	HideModel = 2u,
	Activate = 4u,
	Animate = 8u,
	Depleted = 0x10u,
	Sparkle = 0x20u,
	Stopped = 0x40u,
	NoInteract = 0x80u,
	InvertedMovement = 0x100u,
	LoHighlight = 0x200u
}
