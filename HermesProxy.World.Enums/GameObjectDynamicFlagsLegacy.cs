using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GameObjectDynamicFlagsLegacy : uint
{
	Activate = 1u,
	Animate = 2u,
	NoInteract = 4u,
	Sparkle = 8u,
	Stopped = 0x10u
}
