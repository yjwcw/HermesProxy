using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HermesProxy.Enums;
using HermesProxy.World.Client;

namespace HermesProxy.World.Objects;

public static class UpdateFieldExtensions
{
	private static TypeCode GetTypeCodeOfReturnValue<TK>()
	{
		Type typeFromHandle = typeof(TK);
		TypeCode typeCode = Type.GetTypeCode(typeFromHandle);
		if ((uint)(typeCode - 9) <= 1u || (uint)(typeCode - 13) <= 1u)
		{
			return typeCode;
		}
		typeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(typeFromHandle));
		if ((uint)(typeCode - 9) <= 1u || (uint)(typeCode - 13) <= 1u)
		{
			return typeCode;
		}
		throw new ArgumentException("Type must be one of int, uint, float or its nullable counterpart but was " + typeFromHandle.Name);
	}

	public static TK GetValue<T, TK>(this Dictionary<int, UpdateField> dict, T updateField)
	{
		if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out var value))
		{
			switch (GetTypeCodeOfReturnValue<TK>())
			{
			case TypeCode.UInt32:
				return (TK)(object)value.UInt32Value;
			case TypeCode.Int32:
				return (TK)(object)(int)value.UInt32Value;
			case TypeCode.Single:
				return (TK)(object)value.FloatValue;
			case TypeCode.Double:
				return (TK)(object)(double)value.FloatValue;
			}
		}
		return default(TK);
	}

	public static IEnumerable<TK> GetValue<T, TK>(this Dictionary<int, List<UpdateField>> dict, T updateField)
	{
		if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out var value))
		{
			switch (GetTypeCodeOfReturnValue<TK>())
			{
			case TypeCode.UInt32:
				return value.Select((UpdateField uf) => (TK)(object)uf.UInt32Value);
			case TypeCode.Int32:
				return value.Select((UpdateField uf) => (TK)(object)(int)uf.UInt32Value);
			case TypeCode.Single:
				return value.Select((UpdateField uf) => (TK)(object)uf.FloatValue);
			case TypeCode.Double:
				return value.Select((UpdateField uf) => (TK)(object)(double)uf.FloatValue);
			}
		}
		return Enumerable.Empty<TK>();
	}

	public static TK[] GetArray<T, TK>(this Dictionary<int, UpdateField> dict, T firstUpdateField, int count) where T : Enum
	{
		return dict.GetArray<TK>(LegacyVersion.GetUpdateField(firstUpdateField), count);
	}

	public static TK[] GetArray<TK>(this Dictionary<int, UpdateField> dict, int firstUpdateField, int count)
	{
		TK[] array = new TK[count];
		TypeCode typeCodeOfReturnValue = GetTypeCodeOfReturnValue<TK>();
		for (int i = 0; i < count; i++)
		{
			if (dict != null && dict.TryGetValue(firstUpdateField + i, out var value))
			{
				switch (typeCodeOfReturnValue)
				{
				case TypeCode.UInt32:
					array[i] = (TK)(object)value.UInt32Value;
					break;
				case TypeCode.Int32:
					array[i] = (TK)(object)(int)value.UInt32Value;
					break;
				case TypeCode.Single:
					array[i] = (TK)(object)value.FloatValue;
					break;
				case TypeCode.Double:
					array[i] = (TK)(object)(double)value.FloatValue;
					break;
				}
			}
		}
		return array;
	}

	public static WowGuid GetGuidValue(this Dictionary<int, UpdateField> UpdateFields, int field)
	{
		if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
		{
			uint[] array = UpdateFields.GetArray<uint>(field, 2);
			return new WowGuid64(MathFunctions.MakePair64(array[0], array[1]));
		}
		uint[] array2 = UpdateFields.GetArray<uint>(field, 4);
		return new WowGuid128(MathFunctions.MakePair64(array2[0], array2[1]), MathFunctions.MakePair64(array2[2], array2[3]));
	}

	public static TK GetEnum<T, TK>(this Dictionary<int, UpdateField> dict, T updateField)
	{
		try
		{
			if (dict != null && dict.TryGetValue(LegacyVersion.GetUpdateField(updateField), out var value))
			{
				return (TK)Enum.Parse(typeof(TK).GetGenericArguments()[0], value.UInt32Value.ToString(CultureInfo.InvariantCulture));
			}
		}
		catch (OverflowException)
		{
			return default(TK);
		}
		return default(TK);
	}
}
