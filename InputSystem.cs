using System;
using Microsoft.DirectX.DirectInput;

namespace Ymfas
{
	class InputSystem
	{
		#region PublicEnumerations
		/// <summary>
		/// Encapsulates "logical" key or button state (as opposed
		/// to binary "physical" state)
		/// 
		/// <para>The first time a key/button is pressed, it is
		/// considered <code>Pressed</code>. After that, it is
		/// considered <code>Down</code> until it is released.
		/// Similarly, a key/button is considered <code>Released</code>
		/// the first poll it is up and <code>Up</code> for every
		/// frame after. </para>
		/// </summary>
		public enum State
		{
			Pressed = 0,
			Down = 1,
			Released = 2,
			Up = 3
		}

		/// <summary>
		/// constants for mouse buttons
		/// </summary>
		public enum MouseButton
		{
			Left = 0,
			Right = 1,
			Middle = 2,
			Aux0 = 3,
			Aux1 = 4
		} 
		#endregion

		/// <summary>
		/// Encompasses all position information 
		/// useful for the end-user
		/// </summary>
		public struct MousePosition
		{
			public int X, Y;				// position
			public int dX, dY, dZ;			// change in position since last frame
		}

		#region PrivateFields

		// devices
		private Device keyboard;
		private Device mouse;

		// local key state copy
		private State[] keyboardState;
		private State[] mouseButtonState;

		// mouse-specific information
		private MousePosition mp;

		// DirectInput-specific constants
		private const int KEYBOARD_START = (int)Key.Escape;
		private const int KEYBOARD_END = (int)Key.MediaSelect; 
		#endregion

		public InputSystem(IntPtr hwnd)
		{
			try
			{
				// create the keyboard and mouse(very non-exclusively)
				keyboard = new Device(SystemGuid.Keyboard);
				keyboard.SetCooperativeLevel(hwnd, CooperativeLevelFlags.Background |
					CooperativeLevelFlags.NonExclusive);

				mouse = new Device(SystemGuid.Mouse);
				mouse.SetCooperativeLevel(hwnd,  CooperativeLevelFlags.Background |
					CooperativeLevelFlags.NonExclusive);
				mouse.SetDataFormat(DeviceDataFormat.Mouse);

				// attempt to acquire
				Acquire();

				// allocate space for device state
				keyboardState = new State[KEYBOARD_END + 1];
				mouseButtonState = new State[mouse.CurrentMouseState.GetMouseButtons().Length];
			}
			catch (InputException e)
			{
				Ymfas.Util.RecordException(e);
				throw;
			}
		}

		public void Acquire()
		{
			// try to acquire the devices
			keyboard.Acquire();
			mouse.Acquire();
		}

		#region Update
		/// <summary>
		/// polls the device hardware and updates
		/// all device state information
		/// </summary>
		public void Update()
		{
			keyboard.Poll();
			mouse.Poll();

			// once we do all that, we can
			// update the device states
			UpdateMouse();
			UpdateKeyboard();
		}

		private void UpdateKeyboard()
		{
			KeyboardState ks = keyboard.GetCurrentKeyboardState();

			for (int i = KEYBOARD_START; i < KEYBOARD_END; ++i)
			{
				// change all "Released" to "Up" and
				// all "Pressed" to "Down"
				if (keyboardState[i] == State.Released)
					keyboardState[i] = State.Up;
				if (keyboardState[i] == State.Pressed)
					keyboardState[i] = State.Down;

				// now, update the state based on the recent poll
				keyboardState[i] = ComputeNewState(keyboardState[i], ks[(Key)i]);
			}
		}

		private void UpdateMouse()
		{
			MouseState ms = mouse.CurrentMouseState;
			byte[] buttons = ms.GetMouseButtons();

			// update all the buttons
			for (int i = 0; i < buttons.Length; ++i)
			{
				// change all "Released" to "Up" and
				// all "Pressed" to "Down"
				if (mouseButtonState[i] == State.Released)
					mouseButtonState[i] = State.Up;
				if (mouseButtonState[i] == State.Pressed)
					mouseButtonState[i] = State.Down;

				mouseButtonState[i] = ComputeNewState(mouseButtonState[i], buttons[i] > 0);
			}

			// update mouse position and deltas
			mp.dX = ms.X - mp.X;		// [delta] = [new position] - [old position]
			mp.X = ms.X;
			mp.dY = ms.Y - mp.Y;
			mp.Y = ms.Y;

			// DirectInput.MouseState.Z is a delta, where
			// 120 is the movement for one individual scroll
			mp.dZ = ms.Z / 120;	
		}

		/// <summary>
		/// computes the new logical state based on the previous
		/// logical state and the current physical state.
		/// </summary>
		/// <param name="currState">current logical state; should be only State.Up or State.Down</param>
		/// <param name="down">whether the key/button is currently held down</param>
		/// <returns>new logical state</returns>
		private State ComputeNewState(State currState, bool down)
		{
			if (down)
			{
				if (currState == State.Up)
					return State.Pressed;
				return State.Down;
			}
			else
			{
				if (currState == State.Down)
					return State.Released;
				return State.Up;
			}
		}
		#endregion

		#region KeyboardAccessors
		/** key state queries **/
		public bool IsDown(Key k)
		{ return IsDown(keyboardState[(int)k]); }
		public bool IsPressed(Key k)
		{ return IsPressed(keyboardState[(int)k]); }
		public bool IsUp(Key k)
		{ return IsUp(keyboardState[(int)k]); }
		public bool IsUReleased(Key k)
		{ return IsReleased(keyboardState[(int)k]); }
		#endregion

		#region MouseAccessors
		/** button state queries */
		public bool IsDown(MouseButton b) 
		{ return IsDown(mouseButtonState[(int)b]); }
		public bool IsPressed(MouseButton b) 
		{ return IsPressed(mouseButtonState[(int)b]); }
		public bool IsUp(MouseButton b)
		{ return IsUp(mouseButtonState[(int)b]); }
		public bool IsUReleased(MouseButton b)
		{ return IsReleased(mouseButtonState[(int)b]); }

		/** mouse position queries **/
		public MousePosition Mouse
		{
			get { return this.mp; }
		}
		#endregion

		#region PrivatHelperMethods
		private bool IsDown(State s)
		{ return s == State.Down || s == State.Pressed; }
		private bool IsPressed(State s)
		{ return s == State.Pressed; }
		private bool IsUp(State s)
		{ return s == State.Up || s == State.Released; }
		private bool IsReleased(State s)
		{ return s == State.Released; } 
		#endregion


	}
}