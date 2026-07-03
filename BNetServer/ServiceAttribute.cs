using System;
using Framework.Constants;

namespace BNetServer;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceAttribute : Attribute
{
	public ServiceRequirement Requirement { get; set; }

	public OriginalHash ServiceHash { get; set; }

	public uint MethodId { get; set; }

	public ServiceAttribute(ServiceRequirement requirement, OriginalHash serviceHash, uint methodId)
	{
		Requirement = requirement;
		ServiceHash = serviceHash;
		MethodId = methodId;
	}
}
