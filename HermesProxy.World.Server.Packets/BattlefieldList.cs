using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class BattlefieldList : ServerPacket
{
	public WowGuid128 BattlemasterGuid;

	public int Verification = 121761856;

	public uint BattlemasterListID;

	public byte MinLevel = 70;

	public byte MaxLevel = 70;

	public List<int> BattlefieldInstances = new List<int>();

	public bool PvpAnywhere;

	public bool HasRandomWinToday;

	public BattlefieldList()
		: base(Opcode.SMSG_BATTLEFIELD_LIST)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(BattlemasterGuid);
		_worldPacket.WriteInt32(Verification);
		_worldPacket.WriteUInt32(BattlemasterListID);
		_worldPacket.WriteUInt8(MinLevel);
		_worldPacket.WriteUInt8(MaxLevel);
		_worldPacket.WriteInt32(BattlefieldInstances.Count);
		foreach (int battlefieldInstance in BattlefieldInstances)
		{
			_worldPacket.WriteInt32(battlefieldInstance);
		}
		_worldPacket.WriteBit(PvpAnywhere);
		_worldPacket.WriteBit(HasRandomWinToday);
		_worldPacket.FlushBits();
	}
}
