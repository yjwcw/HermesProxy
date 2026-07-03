using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CharacterLoginFailed : ServerPacket
{
	public LoginFailureReason Code;

	public CharacterLoginFailed()
		: base(Opcode.SMSG_CHARACTER_LOGIN_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)Code);
	}
}
