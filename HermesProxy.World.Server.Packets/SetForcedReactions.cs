using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SetForcedReactions : ServerPacket
{
	public List<ForcedReaction> Reactions = new List<ForcedReaction>();

	public SetForcedReactions()
		: base(Opcode.SMSG_SET_FORCED_REACTIONS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Reactions.Count);
		foreach (ForcedReaction reaction in Reactions)
		{
			reaction.Write(_worldPacket);
		}
	}
}
