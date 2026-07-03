namespace HermesProxy.World.Server.Packets;

internal class PartyInviteResponse : ClientPacket
{
	public byte PartyIndex;

	public bool Accept;

	public uint? RolesDesired;

	public PartyInviteResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		Accept = _worldPacket.HasBit();
		if (_worldPacket.HasBit())
		{
			RolesDesired = _worldPacket.ReadUInt32();
		}
	}
}
