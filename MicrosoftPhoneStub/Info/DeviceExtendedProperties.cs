namespace Microsoft.Phone.Info;

public class DeviceExtendedProperties
{
	public static bool TryGetValue(string key, ref object value)
	{
		switch (key)
		{
			case "DeviceTotalMemory":
				value = 512L * 1024L * 1024L;
				return true;
			default:
				return false;
		}
	}
}
