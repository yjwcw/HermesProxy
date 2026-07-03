using System;

namespace HermesProxy.World.Server.Packets;

public class ChrCustomizationChoice : IComparable<ChrCustomizationChoice>
{
	public uint ChrCustomizationOptionID;

	public uint ChrCustomizationChoiceID;

	public ChrCustomizationChoice(uint optionId, uint chocieId)
	{
		ChrCustomizationOptionID = optionId;
		ChrCustomizationChoiceID = chocieId;
	}

	public void WriteCreate(WorldPacket data)
	{
		data.WriteUInt32(ChrCustomizationOptionID);
		data.WriteUInt32(ChrCustomizationChoiceID);
	}

	public void WriteUpdate(WorldPacket data)
	{
		data.WriteUInt32(ChrCustomizationOptionID);
		data.WriteUInt32(ChrCustomizationChoiceID);
	}

	public int CompareTo(ChrCustomizationChoice other)
	{
		return ChrCustomizationOptionID.CompareTo(other.ChrCustomizationOptionID);
	}
}
