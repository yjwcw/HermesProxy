using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

internal class AuthSession : ClientPacket
{
	public uint RegionID;

	public uint BattlegroupID;

	public uint RealmID;

	public Array<byte> LocalChallenge = new Array<byte>(16);

	public byte[] Digest = new byte[24];

	public ulong DosResponse;

	public string RealmJoinTicket;

	public bool UseIPv6;

	public AuthSession(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		DosResponse = _worldPacket.ReadUInt64();
		RegionID = _worldPacket.ReadUInt32();
		BattlegroupID = _worldPacket.ReadUInt32();
		RealmID = _worldPacket.ReadUInt32();
		for (int i = 0; i < LocalChallenge.GetLimit(); i++)
		{
			LocalChallenge[i] = _worldPacket.ReadUInt8();
		}
		Digest = _worldPacket.ReadBytes(24u);
		UseIPv6 = _worldPacket.HasBit();
		uint num = _worldPacket.ReadUInt32();
		if (num != 0)
		{
			RealmJoinTicket = _worldPacket.ReadString(num);
		}
	}
}
