using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class SpellCastLogData
{
	private long Health;

	private int AttackPower;

	private int SpellPower;

	private uint Armor;

	private List<SpellLogPowerData> PowerData = new List<SpellLogPowerData>();

	public void Write(WorldPacket data)
	{
		data.WriteInt64(Health);
		data.WriteInt32(AttackPower);
		data.WriteInt32(SpellPower);
		data.WriteUInt32(Armor);
		data.WriteBits(PowerData.Count, 9);
		data.FlushBits();
		foreach (SpellLogPowerData powerDatum in PowerData)
		{
			data.WriteInt32(powerDatum.PowerType);
			data.WriteInt32(powerDatum.Amount);
			data.WriteInt32(powerDatum.Cost);
		}
	}
}
