using HermesProxy.World.Enums;

namespace HermesProxy.World;

public abstract class HighGuid
{
	protected HighGuidType highGuidType;

	public HighGuidType GetHighGuidType()
	{
		return highGuidType;
	}
}
