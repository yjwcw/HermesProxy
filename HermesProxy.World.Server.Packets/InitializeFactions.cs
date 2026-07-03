using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InitializeFactions : ServerPacket
{
	private const ushort FactionCount = 400;

	public int[] FactionStandings = new int[400];

	public bool[] FactionHasBonus = new bool[400];

	public ReputationFlags[] FactionFlags = new ReputationFlags[400];

	public InitializeFactions()
		: base(Opcode.SMSG_INITIALIZE_FACTIONS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		for (ushort num = 0; num < 400; num++)
		{
			_worldPacket.WriteUInt8((byte)(FactionFlags[num] & (ReputationFlags.Visible | ReputationFlags.AtWar | ReputationFlags.Hidden | ReputationFlags.Header | ReputationFlags.Peaceful | ReputationFlags.Inactive | ReputationFlags.ShowPropagated | ReputationFlags.HeaderShowsBar)));
			_worldPacket.WriteInt32(FactionStandings[num]);
		}
		for (ushort num2 = 0; num2 < 400; num2++)
		{
			_worldPacket.WriteBit(FactionHasBonus[num2]);
		}
		_worldPacket.FlushBits();
	}
}
