using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sexy;

internal class InputController
{
	public static Vector2 touchPos = Vector2.Zero;

	public static void TestTouchCaps()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		TouchPanelCapabilities capabilities = TouchPanel.GetCapabilities();
		if (capabilities.IsConnected)
		{
			int maximumTouchCount = capabilities.MaximumTouchCount;
			Debug.OutputDebug("maxPoints", maximumTouchCount);
		}
	}

	public static void HandleTouchInput()
	{
	}
}
