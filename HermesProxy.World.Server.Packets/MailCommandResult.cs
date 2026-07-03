using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class MailCommandResult : ServerPacket
{
	public uint MailID;

	public MailActionType Command;

	public MailErrorType ErrorCode;

	public InventoryResult BagResult;

	public uint AttachID;

	public uint QtyInInventory;

	public MailCommandResult()
		: base(Opcode.SMSG_MAIL_COMMAND_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MailID);
		_worldPacket.WriteUInt32((uint)Command);
		_worldPacket.WriteUInt32((uint)ErrorCode);
		_worldPacket.WriteUInt32((uint)BagResult);
		_worldPacket.WriteUInt32(AttachID);
		_worldPacket.WriteUInt32(QtyInInventory);
	}
}
