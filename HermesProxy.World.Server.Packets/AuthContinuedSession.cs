namespace HermesProxy.World.Server.Packets;

internal class AuthContinuedSession : ClientPacket
{
	public ulong DosResponse;

	public ulong Key;

	public byte[] LocalChallenge = new byte[16];

	public byte[] Digest = new byte[24];

	public AuthContinuedSession(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		DosResponse = _worldPacket.ReadUInt64();
		Key = _worldPacket.ReadUInt64();
		LocalChallenge = _worldPacket.ReadBytes(16u);
		Digest = _worldPacket.ReadBytes(24u);
	}
}
