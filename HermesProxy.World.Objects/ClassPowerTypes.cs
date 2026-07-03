using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

public static class ClassPowerTypes
{
	public static sbyte GetPowerSlotForClass(Class classId, PowerType power)
	{
		switch (classId)
		{
		case Class.Warrior:
			switch (power)
			{
			case PowerType.Rage:
				return 0;
			case PowerType.ComboPoints:
				return 1;
			}
			break;
		case Class.Paladin:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Hunter:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Rogue:
			switch (power)
			{
			case PowerType.Energy:
				return 0;
			case PowerType.ComboPoints:
				return 1;
			}
			break;
		case Class.Priest:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Shaman:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Mage:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Warlock:
			if (power == PowerType.Mana)
			{
				return 0;
			}
			break;
		case Class.Druid:
			switch (power)
			{
			case PowerType.Mana:
				return 0;
			case PowerType.Rage:
				return 1;
			case PowerType.Energy:
				return 2;
			case PowerType.ComboPoints:
				return 3;
			}
			break;
		}
		return -1;
	}

	public static sbyte GetPowerSlotForPet(PowerType power)
	{
		return power switch
		{
			PowerType.Focus => 0, 
			PowerType.Happiness => 3, 
			_ => -1, 
		};
	}
}
