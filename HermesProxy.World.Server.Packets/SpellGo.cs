using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellGo : ServerPacket
{
	public SpellCastData Cast = new SpellCastData();

	public SpellCastLogData LogData;

	public SpellGo()
		: base(Opcode.SMSG_SPELL_GO, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		Cast.Write(_worldPacket);
		_worldPacket.WriteBit(LogData != null);
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
		_worldPacket.FlushBits();
	}
}
