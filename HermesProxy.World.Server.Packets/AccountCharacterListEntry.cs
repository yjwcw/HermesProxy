using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class AccountCharacterListEntry
{
	public WowGuid128 AccountId;

	public uint RealmVirtualAddress;

	public string RealmName;

	public WowGuid128 CharacterGuid;

	public string Name;

	public Race Race;

	public Class Class;

	public Gender Sex;

	public byte Level;

	public ulong LastLoginUnixSec;

	public uint Unk;

	public void Write(WorldPacket packet)
	{
		packet.WritePackedGuid128(AccountId);
		packet.WritePackedGuid128(CharacterGuid);
		packet.WriteUInt32(RealmVirtualAddress);
		packet.WriteUInt8((byte)Race);
		packet.WriteUInt8((byte)Class);
		packet.WriteUInt8((byte)Sex);
		packet.WriteUInt8(Level);
		packet.WriteUInt64(LastLoginUnixSec);
		if (ModernVersion.AddedInClassicVersion(1, 14, 1, 2, 5, 3))
		{
			packet.WriteUInt32(Unk);
		}
		packet.ResetBitPos();
		packet.WriteBits(Name.GetByteCount(), 6);
		packet.WriteBits(RealmName.GetByteCount(), 9);
		packet.WriteString(Name);
		packet.WriteString(RealmName);
	}
}
