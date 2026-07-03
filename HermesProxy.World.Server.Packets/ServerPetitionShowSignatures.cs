using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ServerPetitionShowSignatures : ServerPacket
{
	public struct PetitionSignature
	{
		public WowGuid128 Signer;

		public int Choice;
	}

	public WowGuid128 Item;

	public WowGuid128 Owner;

	public WowGuid128 OwnerAccountID;

	public int PetitionID;

	public List<PetitionSignature> Signatures;

	public ServerPetitionShowSignatures()
		: base(Opcode.SMSG_PETITION_SHOW_SIGNATURES)
	{
		Signatures = new List<PetitionSignature>();
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Item);
		_worldPacket.WritePackedGuid128(Owner);
		_worldPacket.WritePackedGuid128(OwnerAccountID);
		_worldPacket.WriteInt32(PetitionID);
		_worldPacket.WriteInt32(Signatures.Count);
		foreach (PetitionSignature signature in Signatures)
		{
			_worldPacket.WritePackedGuid128(signature.Signer);
			_worldPacket.WriteInt32(signature.Choice);
		}
	}
}
