using System.Runtime.InteropServices;

namespace HermesProxy.World.Objects;

[StructLayout(LayoutKind.Explicit)]
public struct UpdateValues
{
	[FieldOffset(0)]
	public uint UnsignedValue;

	[FieldOffset(0)]
	public int SignedValue;

	[FieldOffset(0)]
	public float FloatValue;
}
