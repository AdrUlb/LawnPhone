using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Lawn;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sexy;

public class Main : Game
{
	private static SexyTransform2D orientationTransform;

	private static UI_ORIENTATION orientationUsed;

	private static bool newOrientation;

	public static GamerServicesComponent GamerServicesComp;

	public static bool trialModeChecked = false;

	private static bool trialModeCachedValue = true;

	internal static Graphics graphics;

	private int mFrameCnt;

	private static bool startedProfiler;

	private static bool wantToSuppressDraw;

	private GamePadState previousGamepadState = default(GamePadState);

	public static bool RunWhenLocked
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			return (int)PhoneApplicationService.Current.ApplicationIdleDetectionMode == 1;
		}
		set
		{
			try
			{
				PhoneApplicationService.Current.ApplicationIdleDetectionMode = (IdleDetectionMode)(value ? 1 : 0);
			}
			catch
			{
			}
		}
	}

	public static bool LOW_MEMORY_DEVICE { get; private set; }

	public static bool DO_LOW_MEMORY_OPTIONS { get; private set; }

	public static bool IsInTrialMode => trialModeCachedValue;

	public Main()
	{
		SetupTileSchedule();
		graphics = Graphics.GetNew(this);
		SetLowMem();
		graphics.IsFullScreen = false;
		Guide.SimulateTrialMode = false;
		graphics.PreferredBackBufferWidth = 800;
		graphics.PreferredBackBufferHeight = 480;
		GraphicsState.mGraphicsDeviceManager.SupportedOrientations = Constants.SupportedOrientations;
		GraphicsState.mGraphicsDeviceManager.DeviceCreated += graphics_DeviceCreated;
		GraphicsState.mGraphicsDeviceManager.DeviceReset += graphics_DeviceReset;
		GraphicsState.mGraphicsDeviceManager.PreparingDeviceSettings += mGraphicsDeviceManager_PreparingDeviceSettings;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);
		Exiting += Main_Exiting;
		PhoneApplicationService.Current.UserIdleDetectionMode = 0;
		PhoneApplicationService.Current.Launching += Game_Launching;
		PhoneApplicationService.Current.Activated += Game_Activated;
		PhoneApplicationService.Current.Closing += Current_Closing;
		PhoneApplicationService.Current.Deactivated += Current_Deactivated;
	}

	private void Current_Deactivated(object sender, DeactivatedEventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.Tombstoned();
	}

	private void Current_Closing(object sender, ClosingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}

	private void Game_Activated(object sender, ActivatedEventArgs e)
	{
	}

	private void Game_Launching(object sender, LaunchingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}

	private static void SetupTileSchedule()
	{
	}

	private void mGraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
	}

	private void graphics_DeviceReset(object sender, EventArgs e)
	{
	}

	private void graphics_DeviceCreated(object sender, EventArgs e)
	{
		((Game)this).GraphicsDevice.PresentationParameters.PresentationInterval = (PresentInterval)3;
	}

	private void Main_Exiting(object sender, EventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.AppExit();
	}

	protected override void Initialize()
	{
		Window.OrientationChanged += Window_OrientationChanged;
		GamerServicesComp = new GamerServicesComponent((Game)(object)this);
		ReportAchievement.Initialise();
		base.Initialize();
	}

	protected override void LoadContent()
	{
		GraphicsState.Init();
		SetupForResolution();
		GlobalStaticVars.initialize(this);
		GlobalStaticVars.mGlobalContent.LoadSplashScreen();
		GlobalStaticVars.gSexyAppBase.StartLoadingThread();
	}

	protected override void UnloadContent()
	{
		GlobalStaticVars.mGlobalContent.cleanUp();
	}

	public void CompensateForSlowUpdate()
	{
		ResetElapsedTime();
	}

	protected override void Update(GameTime gameTime)
	{
		if (!((Game)this).IsActive)
		{
			return;
		}
		if (GlobalStaticVars.gSexyAppBase.WantsToExit)
		{
			((Game)this).Exit();
		}
		HandleInput(gameTime);
		GlobalStaticVars.gSexyAppBase.UpdateApp();
		if (!trialModeChecked)
		{
			trialModeChecked = true;
			bool flag = trialModeCachedValue;
			SetLowMem();
			trialModeCachedValue = Guide.IsTrialMode;
			if (flag != trialModeCachedValue && flag)
			{
				LeftTrialMode();
			}
		}
		try
		{
			base.Update(gameTime);
		}
		catch (GameUpdateRequiredException)
		{
			GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
		}
	}

	private static void SetLowMem()
	{
		object obj = default(object);
		DeviceExtendedProperties.TryGetValue("DeviceTotalMemory", ref obj);
		DO_LOW_MEMORY_OPTIONS = (LOW_MEMORY_DEVICE = (long)obj / 1024 / 1024 <= 256);
		LOW_MEMORY_DEVICE = false;
	}

	private void LeftTrialMode()
	{
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.LeftTrialMode();
		}
		Window_OrientationChanged(null, null);
	}

	public static void SuppressNextDraw()
	{
		wantToSuppressDraw = true;
	}

	public static SignedInGamer GetGamer()
	{
		if (((ReadOnlyCollection<SignedInGamer>)(object)Gamer.SignedInGamers).Count == 0)
		{
			return null;
		}
		return Gamer.SignedInGamers[(PlayerIndex)0];
	}

	public static void NeedToSetUpOrientationMatrix(UI_ORIENTATION orientation)
	{
		orientationUsed = orientation;
		newOrientation = true;
	}

	private static void SetupOrientationMatrix(UI_ORIENTATION orientation)
	{
		newOrientation = false;
	}

	private void Window_OrientationChanged(object sender, EventArgs e)
	{
		SetupInterfaceOrientation();
	}

	private void SetupInterfaceOrientation()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			if ((int)((Game)this).Window.CurrentOrientation == 1 || (int)((Game)this).Window.CurrentOrientation == 2)
			{
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_LANDSCAPE_LEFT);
			}
			else
			{
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_PORTRAIT);
			}
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (newOrientation)
		{
			SetupOrientationMatrix(orientationUsed);
		}
		lock (ResourceManager.DrawLocker)
		{
			GraphicsDevice.Clear(Color.Black);
			GlobalStaticVars.gSexyAppBase.DrawGame(gameTime);
			base.Draw(gameTime);
		}
	}

	public void HandleInput(GameTime gameTime)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Invalid comparison between Unknown and I4
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Invalid comparison between Unknown and I4
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Invalid comparison between Unknown and I4
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (LoadingScreen.IsLoading)
		{
			return;
		}
		GamePadState state = GamePad.GetState((PlayerIndex)0);
		GamePadButtons buttons = state.Buttons;
		if (buttons.Back == ButtonState.Pressed)
		{
			GamePadButtons buttons2 = previousGamepadState.Buttons;
			if (buttons2.Back == ButtonState.Released)
			{
				GlobalStaticVars.gSexyAppBase.BackButtonPress();
			}
		}
		TouchCollection state2 = TouchPanel.GetState();
		bool flag = false;
		var enumerator = state2.GetEnumerator();
		try
		{
			TouchLocation val = default(TouchLocation);
			while (enumerator.MoveNext())
			{
				TouchLocation current = enumerator.Current;
				_Touch touch = default(_Touch);
				touch.location.mX = current.Position.X;
				touch.location.mY = current.Position.Y;
				if (current.TryGetPreviousLocation(out val))
				{
					touch.previousLocation = new CGPoint(val.Position.X, val.Position.Y);
				}
				else
				{
					touch.previousLocation = touch.location;
				}
				touch.timestamp = gameTime.TotalGameTime.TotalSeconds;
				if (current.State == TouchLocationState.Pressed && !flag)
				{
					GlobalStaticVars.gSexyAppBase.TouchBegan(touch);
					flag = true;
				}
				else if (current.State == TouchLocationState.Moved)
				{
					GlobalStaticVars.gSexyAppBase.TouchMoved(touch);
				}
				else if (current.State == TouchLocationState.Released)
				{
					GlobalStaticVars.gSexyAppBase.TouchEnded(touch);
				}
				else if (current.State == TouchLocationState.Invalid)
				{
					GlobalStaticVars.gSexyAppBase.TouchesCanceled();
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		previousGamepadState = state;
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		trialModeChecked = false;
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.GotFocus();
			if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
			{
				GlobalStaticVars.gSexyAppBase.mMusicInterface.ResumeMusic();
			}
		}
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		GlobalStaticVars.gSexyAppBase.LostFocus();
		if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
		{
			GlobalStaticVars.gSexyAppBase.mMusicInterface.PauseMusic();
		}
		GlobalStaticVars.gSexyAppBase.AppEnteredBackground();
		base.OnDeactivated(sender, args);
	}

	private void GameSpecificCheatInputCheck()
	{
	}

	private static void SetupForResolution()
	{
		Strings.Culture = CultureInfo.CurrentCulture;
		if (Strings.Culture.TwoLetterISOLanguageName == "fr")
		{
			Constants.Language = Constants.LanguageIndex.fr;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "de")
		{
			Constants.Language = Constants.LanguageIndex.de;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "es")
		{
			Constants.Language = Constants.LanguageIndex.es;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "it")
		{
			Constants.Language = Constants.LanguageIndex.it;
		}
		else
		{
			Constants.Language = Constants.LanguageIndex.en;
		}
		if ((graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 480 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 800) || (graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 800 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 480))
		{
			AtlasResources.mAtlasResources = new AtlasResources_480x800();
			Constants.Load480x800();
			return;
		}
		throw new Exception("Unsupported Resolution");
	}
}
