using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ConquestFormulaConstants : ServerPacket
{
	public int PvpMinCPPerWeek;

	public int PvpMaxCPPerWeek;

	public float PvpCPBaseCoefficient;

	public float PvpCPExpCoefficient;

	public float PvpCPNumerator;

	public ConquestFormulaConstants()
		: base(Opcode.SMSG_CONQUEST_FORMULA_CONSTANTS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(PvpMinCPPerWeek);
		_worldPacket.WriteInt32(PvpMaxCPPerWeek);
		_worldPacket.WriteFloat(PvpCPBaseCoefficient);
		_worldPacket.WriteFloat(PvpCPExpCoefficient);
		_worldPacket.WriteFloat(PvpCPNumerator);
	}
}
