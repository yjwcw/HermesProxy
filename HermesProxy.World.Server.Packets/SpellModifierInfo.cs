using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class SpellModifierInfo
{
	public byte ModIndex;

	public List<SpellModifierData> ModifierData = new List<SpellModifierData>();

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(ModIndex);
		data.WriteInt32(ModifierData.Count);
		foreach (SpellModifierData modifierDatum in ModifierData)
		{
			modifierDatum.Write(data);
		}
	}
}
