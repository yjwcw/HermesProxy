using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class PartyMemberPhaseStates
{
	public struct PartyMemberPhase
	{
		public ushort PhaseFlags;

		public ushort Id;

		public void Write(WorldPacket data)
		{
			data.WriteUInt16(PhaseFlags);
			data.WriteUInt16(Id);
		}
	}

	public uint PhaseShiftFlags;

	public List<PartyMemberPhase> Phases = new List<PartyMemberPhase>();

	public WowGuid128 PersonalGUID = WowGuid128.Empty;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(PhaseShiftFlags);
		data.WriteInt32(Phases.Count);
		data.WritePackedGuid128(PersonalGUID);
		foreach (PartyMemberPhase phase in Phases)
		{
			phase.Write(data);
		}
	}
}
