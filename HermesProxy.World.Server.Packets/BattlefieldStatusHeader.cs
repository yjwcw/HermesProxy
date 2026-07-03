using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class BattlefieldStatusHeader
{
	public RideTicket Ticket = new RideTicket();

	public List<uint> BattlefieldListIDs = new List<uint>();

	public byte Unk254;

	public byte RangeMin;

	public byte RangeMax = 70;

	public byte ArenaTeamSize;

	public uint InstanceID;

	public bool IsArena;

	public bool TournamentRules;

	public void Write(WorldPacket data)
	{
		Ticket.Write(data);
		if (ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4))
		{
			data.WriteUInt8(Unk254);
		}
		data.WriteInt32(BattlefieldListIDs.Count);
		data.WriteUInt8(RangeMin);
		data.WriteUInt8(RangeMax);
		data.WriteUInt8(ArenaTeamSize);
		data.WriteUInt32(InstanceID);
		foreach (uint battlefieldListID in BattlefieldListIDs)
		{
			ulong data2 = battlefieldListID | 0x1F10000000000000uL;
			data.WriteUInt64(data2);
		}
		data.WriteBit(IsArena);
		data.WriteBit(TournamentRules);
		data.FlushBits();
	}
}
