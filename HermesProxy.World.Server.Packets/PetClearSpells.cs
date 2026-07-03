using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PetClearSpells : ServerPacket
{
	public PetClearSpells()
		: base(Opcode.SMSG_PET_CLEAR_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
	}
}
