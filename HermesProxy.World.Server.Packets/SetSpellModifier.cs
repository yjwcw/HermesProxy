using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SetSpellModifier : ServerPacket
{
	public List<SpellModifierInfo> Modifiers = new List<SpellModifierInfo>();

	public SetSpellModifier(Opcode opcode)
		: base(opcode, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Modifiers.Count);
		foreach (SpellModifierInfo modifier in Modifiers)
		{
			modifier.Write(_worldPacket);
		}
	}
}
