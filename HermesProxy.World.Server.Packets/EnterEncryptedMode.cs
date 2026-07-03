using System;
using System.Linq;
using System.Security.Cryptography;
using Framework.Cryptography;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class EnterEncryptedMode : ServerPacket
{
	private byte[] EncryptionKey;

	private bool Enabled;

	private static byte[] EnableEncryptionSeed = new byte[16]
	{
		144, 156, 208, 80, 90, 44, 20, 221, 92, 44,
		192, 100, 20, 243, 254, 201
	};

	public EnterEncryptedMode(byte[] encryptionKey, bool enabled)
		: base(Opcode.SMSG_ENTER_ENCRYPTED_MODE)
	{
		EncryptionKey = encryptionKey;
		Enabled = enabled;
	}

	public override void Write()
	{
		HmacSha256 hmacSha = new HmacSha256(EncryptionKey);
		hmacSha.Process(BitConverter.GetBytes(Enabled), 1);
		hmacSha.Finish(EnableEncryptionSeed, 16);
		_worldPacket.WriteBytes(RsaCrypt.RSA.SignHash(hmacSha.Digest, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).Reverse().ToArray());
		_worldPacket.WriteBit(Enabled);
		_worldPacket.FlushBits();
	}
}
