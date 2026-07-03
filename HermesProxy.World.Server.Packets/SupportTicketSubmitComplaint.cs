using System;
using System.Collections.Generic;
using Framework.GameMath;
using Framework.Logging;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SupportTicketSubmitComplaint : ClientPacket
{
	public class HeaderInfo
	{
		public uint SelfPlayerMapId;

		public Vector3 SelfPlayerPos;

		public float SelfPlayerOrientation;

		public void Read(WorldPacket worldPacket)
		{
			SelfPlayerMapId = worldPacket.ReadUInt32();
			SelfPlayerPos = worldPacket.ReadVector3();
			SelfPlayerOrientation = worldPacket.ReadFloat();
		}
	}

	public class ChatLogInfo
	{
		public class ChatLine
		{
			public DateTime Time;

			public string Text;
		}

		public List<ChatLine> ChatLines = new List<ChatLine>();

		public uint? ReportedLineIdx;

		public void Read(WorldPacket worldPacket)
		{
			uint num = worldPacket.ReadUInt32();
			bool flag = worldPacket.ReadBool();
			for (int i = 0; i < num; i++)
			{
				DateTime time = worldPacket.ReadTime64();
				uint length = worldPacket.ReadBits<uint>(12);
				worldPacket.ResetBitPos();
				string text = worldPacket.ReadString(length);
				ChatLines.Add(new ChatLine
				{
					Time = time,
					Text = text
				});
			}
			if (flag)
			{
				ReportedLineIdx = worldPacket.ReadUInt32();
			}
		}
	}

	public class MailInfo
	{
		public uint MailId;

		public string MailTextBody;

		public string MailSubject;

		public void Read(WorldPacket worldPacket)
		{
			MailId = worldPacket.ReadUInt32();
			uint length = worldPacket.ReadBits<uint>(13);
			uint length2 = worldPacket.ReadBits<uint>(9);
			worldPacket.ResetBitPos();
			MailTextBody = worldPacket.ReadString(length);
			MailSubject = worldPacket.ReadString(length2);
		}
	}

	public HeaderInfo Header = new HeaderInfo();

	public WowGuid128 TargetCharacterGuid;

	public ChatLogInfo ChatLog = new ChatLogInfo();

	public MailInfo? SelectedMailInfo;

	public GmTicketComplaintType ComplaintType;

	public string TextNote;

	public SupportTicketSubmitComplaint(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Header.Read(_worldPacket);
		TargetCharacterGuid = _worldPacket.ReadPackedGuid128();
		ChatLog.Read(_worldPacket);
		ComplaintType = (GmTicketComplaintType)_worldPacket.ReadBits<uint>(5);
		uint length = _worldPacket.ReadBits<uint>(10);
		bool flag = _worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ReadBit();
		bool num = _worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ReadBit();
		_worldPacket.ResetBitPos();
		if (num)
		{
			_worldPacket.ReadBit();
			_worldPacket.ResetBitPos();
		}
		if (_worldPacket.ReadUInt32() != 0)
		{
			Log.Print(LogType.Error, "You reported something that we do not handle (?)", "Read", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\Packets\\SupportTicketPackets.cs");
			Log.Print(LogType.Error, "Please create a new issue on GitHub and tell us what you did", "Read", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\Packets\\SupportTicketPackets.cs");
			return;
		}
		if (flag)
		{
			SelectedMailInfo = new MailInfo();
			SelectedMailInfo.Read(_worldPacket);
		}
		TextNote = _worldPacket.ReadString(length);
	}
}
