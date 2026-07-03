using System;
using HermesProxy.Enums;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class PacketHandlerAttribute : Attribute
{
	public Opcode Opcode { get; private set; }

	public PacketHandlerAttribute(Opcode opcode)
	{
		Opcode = opcode;
	}

	public PacketHandlerAttribute(uint opcode)
	{
		Opcode = (Opcode)opcode;
	}

	public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion)
	{
		if (LegacyVersion.AddedInVersion(addedInVersion))
		{
			Opcode = opcode;
		}
	}

	public PacketHandlerAttribute(Opcode opcode, ClientVersionBuild addedInVersion, ClientVersionBuild removedInVersion)
	{
		if (LegacyVersion.InVersion(addedInVersion, removedInVersion))
		{
			Opcode = opcode;
		}
	}
}
