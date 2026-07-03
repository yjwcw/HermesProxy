using System;
using Framework.Collections;
using Framework.IO;
using Framework.Logging;

namespace HermesProxy.World.Objects;

public class UpdateFieldsArray
{
	public uint ValuesCount;

	public UpdateValues[] m_updateValues;

	public UpdateMask m_updateMask;

	public UpdateFieldsArray(uint size)
	{
		ValuesCount = size;
		m_updateValues = new UpdateValues[size];
		m_updateMask = new UpdateMask(size);
	}

	public void WriteToPacket(ByteBuffer buffer)
	{
		ByteBuffer byteBuffer = new ByteBuffer();
		for (int i = 0; i < ValuesCount; i++)
		{
			if (m_updateMask.GetBit(i))
			{
				byteBuffer.WriteUInt32(m_updateValues[i].UnsignedValue);
			}
		}
		m_updateMask.AppendToPacket(buffer);
		buffer.WriteBytes(byteBuffer);
	}

	public void SetUpdateField<T>(object index, T value, byte offset = 0) where T : new()
	{
		if (value is byte b)
		{
			if (offset > 3)
			{
				Log.Print(LogType.Error, $"SetUpdateField<UInt8>: Wrong offset: {offset}", "SetUpdateField", "D:\\a\\HermesProxy\\HermesProxy\\World\\Objects\\UpdateFieldsArray.cs");
			}
			else if ((byte)(m_updateValues[(int)index].UnsignedValue >> offset * 8) != b)
			{
				m_updateValues[(int)index].UnsignedValue &= (uint)(~(255 << offset * 8));
				m_updateValues[(int)index].UnsignedValue |= (uint)(b << offset * 8);
				m_updateMask.SetBit((int)index);
			}
		}
		else if (value is ushort num)
		{
			if (offset > 1)
			{
				Log.Print(LogType.Error, $"SetUpdateField<UInt16>: Wrong offset: {offset}", "SetUpdateField", "D:\\a\\HermesProxy\\HermesProxy\\World\\Objects\\UpdateFieldsArray.cs");
			}
			else if ((ushort)(GetUpdateField<uint>(index, 0) >> offset * 16) != num)
			{
				m_updateValues[(int)index].UnsignedValue &= (uint)(~(65535 << offset * 16));
				m_updateValues[(int)index].UnsignedValue |= (uint)(num << offset * 16);
				m_updateMask.SetBit((int)index);
			}
		}
		else if (value is int num2)
		{
			if (m_updateValues[(int)index].SignedValue != num2)
			{
				m_updateValues[(int)index].SignedValue = num2;
				m_updateMask.SetBit((int)index);
			}
		}
		else if (value is uint num3)
		{
			if (m_updateValues[(int)index].UnsignedValue != num3)
			{
				m_updateValues[(int)index].UnsignedValue = num3;
				m_updateMask.SetBit((int)index);
			}
		}
		else if (value is float num4)
		{
			if (m_updateValues[(int)index].FloatValue != num4)
			{
				m_updateValues[(int)index].FloatValue = num4;
				m_updateMask.SetBit((int)index);
			}
		}
		else if (value is ulong num5)
		{
			if (GetUpdateField<ulong>(index, 0) != num5)
			{
				m_updateValues[(int)index].UnsignedValue = MathFunctions.Pair64_LoPart(num5);
				m_updateValues[(int)index + 1].UnsignedValue = MathFunctions.Pair64_HiPart(num5);
				m_updateMask.SetBit((int)index);
				m_updateMask.SetBit((int)index + 1);
			}
		}
		else
		{
			if (!(value is WowGuid128 wowGuid))
			{
				throw new Exception("Unhandled type " + typeof(T).ToString() + " in SetUpdateField!");
			}
			SetUpdateField(index, wowGuid.GetLowValue(), 0);
			SetUpdateField((int)index + 2, wowGuid.GetHighValue(), 0);
		}
	}

	public T GetUpdateField<T>(object index, byte offset = 0)
	{
		T val = default(T);
		if (!(val is byte))
		{
			if (!(val is ushort))
			{
				if (!(val is int))
				{
					if (!(val is uint))
					{
						if (!(val is float))
						{
							if (!(val is ulong))
							{
								if (val is WowGuid128)
								{
									return (T)Convert.ChangeType(new WowGuid128(GetUpdateField<ulong>((int)index + 2, 0), GetUpdateField<ulong>(index, 0)), typeof(T));
								}
								throw new Exception($"{typeof(T)} is not implemented in GetUpdateField<T>");
							}
							return (T)Convert.ChangeType(((ulong)m_updateValues[(int)index + 1].UnsignedValue << 32) | m_updateValues[(int)index].UnsignedValue, typeof(T));
						}
						return (T)Convert.ChangeType(m_updateValues[(int)index].FloatValue, typeof(T));
					}
					return (T)Convert.ChangeType(m_updateValues[(int)index].UnsignedValue, typeof(T));
				}
				return (T)Convert.ChangeType(m_updateValues[(int)index].SignedValue, typeof(T));
			}
			return (T)Convert.ChangeType((ushort)(m_updateValues[(int)index].UnsignedValue >> offset * 16) & 0xFFFF, typeof(T));
		}
		return (T)Convert.ChangeType((byte)(m_updateValues[(int)index].UnsignedValue >> offset * 8) & 0xFF, typeof(T));
	}

	public void _LoadIntoDataField(string data, uint startOffset, uint count)
	{
		if (string.IsNullOrEmpty(data))
		{
			return;
		}
		StringArray stringArray = new StringArray(data, ' ');
		if (stringArray.Length != count)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			if (uint.TryParse(stringArray[i], out var result))
			{
				m_updateValues[(int)startOffset + i].UnsignedValue = result;
				m_updateMask.SetBit((int)(startOffset + i));
			}
		}
	}

	public bool HasFlag(object index, object flag)
	{
		if ((int)index >= ValuesCount)
		{
			return false;
		}
		return (GetUpdateField<uint>(index, 0) & (uint)flag) != 0;
	}

	public void AddFlag(object index, object newFlag)
	{
		uint unsignedValue = m_updateValues[(int)index].UnsignedValue;
		uint num = unsignedValue | Convert.ToUInt32(newFlag);
		if (unsignedValue != num)
		{
			SetUpdateField(index, num, 0);
		}
	}

	public void RemoveFlag(object index, object newFlag)
	{
		uint unsignedValue = m_updateValues[(int)index].UnsignedValue;
		uint num = unsignedValue & ~Convert.ToUInt32(newFlag);
		if (unsignedValue != num)
		{
			SetUpdateField(index, num, 0);
		}
	}

	public void ApplyFlag<T>(object index, T flag, bool apply)
	{
		if (apply)
		{
			AddFlag(index, flag);
		}
		else
		{
			RemoveFlag(index, flag);
		}
	}

	public void AddFlag64(object index, object newFlag)
	{
		ulong updateField = GetUpdateField<ulong>(index, 0);
		ulong num = updateField | Convert.ToUInt64(newFlag);
		if (updateField != num)
		{
			SetUpdateField(index, num, 0);
		}
	}

	public void RemoveFlag64(object index, object newFlag)
	{
		ulong updateField = GetUpdateField<ulong>(index, 0);
		ulong num = updateField & ~Convert.ToUInt64(newFlag);
		if (updateField != num)
		{
			SetUpdateField(index, num, 0);
		}
	}

	public void ApplyFlag64<T>(object index, T flag, bool apply)
	{
		if (apply)
		{
			AddFlag(index, flag);
		}
		else
		{
			RemoveFlag(index, flag);
		}
	}

	public void AddByteFlag(object index, byte offset, object newFlag)
	{
		if (offset > 4)
		{
			Log.Print(LogType.Error, $"Object.SetByteFlag: Wrong offset {offset}", "AddByteFlag", "D:\\a\\HermesProxy\\HermesProxy\\World\\Objects\\UpdateFieldsArray.cs");
		}
		else if ((((byte)m_updateValues[(int)index].UnsignedValue >> offset * 8) & (int)newFlag) == 0)
		{
			m_updateValues[(int)index].UnsignedValue |= (uint)newFlag << offset * 8;
			m_updateMask.SetBit((int)index);
		}
	}

	public void RemoveByteFlag(object index, byte offset, object oldFlag)
	{
		if (offset > 4)
		{
			Log.Print(LogType.Error, $"Object.RemoveByteFlag: Wrong offset {offset}", "RemoveByteFlag", "D:\\a\\HermesProxy\\HermesProxy\\World\\Objects\\UpdateFieldsArray.cs");
		}
		else if ((((byte)m_updateValues[(int)index].UnsignedValue >> offset * 8) & (int)oldFlag) != 0)
		{
			m_updateValues[(int)index].UnsignedValue &= ~((uint)oldFlag << offset * 8);
			m_updateMask.SetBit((int)index);
		}
	}
}
