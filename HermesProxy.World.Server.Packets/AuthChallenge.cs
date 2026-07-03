using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuthChallenge : ServerPacket
{
	public byte[] Challenge = new byte[16];

	public byte[] DosChallenge = new byte[32];

	public byte DosZeroBits;

	public AuthChallenge()
		: base(Opcode.SMSG_AUTH_CHALLENGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBytes(DosChallenge);
		_worldPacket.WriteBytes(Challenge);
		_worldPacket.WriteUInt8(DosZeroBits);
	}
}
