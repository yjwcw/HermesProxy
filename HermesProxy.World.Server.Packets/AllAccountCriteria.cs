using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AllAccountCriteria : ServerPacket
{
	public List<CriteriaProgressPkt> Progress = new List<CriteriaProgressPkt>();

	public AllAccountCriteria()
		: base(Opcode.SMSG_ALL_ACCOUNT_CRITERIA, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Progress.Count);
		foreach (CriteriaProgressPkt item in Progress)
		{
			item.Write(_worldPacket);
		}
	}
}
