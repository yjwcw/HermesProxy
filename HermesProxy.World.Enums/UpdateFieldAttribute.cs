using System;
using HermesProxy.Enums;

namespace HermesProxy.World.Enums;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class UpdateFieldAttribute : Attribute
{
	public UpdateFieldType UFAttribute { get; private set; }

	public ClientVersionBuild Version { get; private set; }

	public UpdateFieldAttribute(UpdateFieldType attrib)
	{
		UFAttribute = attrib;
		Version = ClientVersionBuild.Zero;
	}

	public UpdateFieldAttribute(UpdateFieldType attrib, ClientVersionBuild fromVersion)
	{
		UFAttribute = attrib;
		Version = fromVersion;
	}
}
