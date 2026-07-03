using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayerModelDisplayInfo
{
	public WowGuid128 GUID;

	public List<InspectItemData> Items = new List<InspectItemData>();

	public string Name;

	public uint SpecializationID;

	public Gender SexId;

	public Race RaceId;

	public Class ClassId;

	public List<ChrCustomizationChoice> Customizations = new List<ChrCustomizationChoice>();

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(GUID);
		data.WriteUInt32(SpecializationID);
		data.WriteInt32(Items.Count);
		data.WriteBits(Name.GetByteCount(), 6);
		data.WriteUInt8((byte)SexId);
		data.WriteUInt8((byte)RaceId);
		data.WriteUInt8((byte)ClassId);
		data.WriteInt32(Customizations.Count);
		data.WriteString(Name);
		foreach (ChrCustomizationChoice customization in Customizations)
		{
			data.WriteUInt32(customization.ChrCustomizationOptionID);
			data.WriteUInt32(customization.ChrCustomizationChoiceID);
		}
		foreach (InspectItemData item in Items)
		{
			item.Write(data);
		}
	}
}
