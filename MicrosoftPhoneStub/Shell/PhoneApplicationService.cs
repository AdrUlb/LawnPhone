namespace Microsoft.Phone.Shell;

public class LaunchingEventArgs : EventArgs;
public class ActivatedEventArgs : EventArgs;
public class ClosingEventArgs : EventArgs;
public class DeactivatedEventArgs : EventArgs;

public class PhoneApplicationService
{
	public static PhoneApplicationService Current { get; private set; } = new();

	public IdleDetectionMode ApplicationIdleDetectionMode { get; set; }

	public IdleDetectionMode UserIdleDetectionMode { get; set; }

	public IDictionary<string, object> State { get; } = new Dictionary<string, object>();

	public event EventHandler<LaunchingEventArgs> Launching;

	public event EventHandler<ActivatedEventArgs> Activated;

	public event EventHandler<ClosingEventArgs> Closing;

	public event EventHandler<DeactivatedEventArgs> Deactivated;
}
