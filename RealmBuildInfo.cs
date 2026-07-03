using System.Collections.Generic;

public class RealmBuildInfo
{
	public uint Build;

	public uint MajorVersion;

	public uint MinorVersion;

	public uint BugfixVersion;

	public char[] HotfixVersion = new char[4];

	public byte[] FallbackStaticSeed = new byte[16];

	public Dictionary<string, byte[]> BuildSeeds = new Dictionary<string, byte[]>();
}
