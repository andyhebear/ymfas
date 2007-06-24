using System;
using IrrlichtNETCP;

namespace Ymfas
{	
	/// <summary>
	/// Provides an easy interface for interpreting
	/// keyboard and mouse input
	/// </summary>
	class IrrInputSystem
	{
		public enum ButtonCode
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}
		
		public enum KeyState
		{
			Down = 0,
			Up = 1,
			Pressed = 2,
			Released = 3
		}
		
		public KeyState [] keyboardStates;
		private KeyState [] mouseButtonStates;

		private bool gatheringInput;
		private Position2D mouseCoords, mouseDelta;
		private float wheelDelta;
		
		public IrrInputSystem()
		{
			keyboardStates = new KeyState[(int)IrrlichtNETCP.KeyCode.CODES_COUNT];
			mouseButtonStates = new KeyState[3];
			
			// initialize the keyboard and mouse button states and coordinates
			for (int i = 0; i < keyboardStates.Length; ++i)
				keyboardStates[i] = KeyState.Up;
			for (int i = 0; i < mouseButtonStates.Length; ++i)
				mouseButtonStates[i] = KeyState.Up;
		 
			mouseCoords = new Position2D();
			mouseDelta = new Position2D();
			wheelDelta = 0;

			// initially gathering input
			gatheringInput = true;
		}

		public bool OnEvent(Event ev)
		{
			// only process events when not in
			// the main input loop
			if (!gatheringInput)
				return false;

			// for a keyboard event, we just need the new keystate
			if (ev.Type == EventType.KeyInputEvent)
			{
				// assign the new key state
				keyboardStates[(int)ev.KeyCode] =
					FindNewKeyState(keyboardStates[(int)ev.KeyCode],
				    				ev.KeyPressedDown);

				// signal that we handled the event
				return true;
			}
			
			// for mouse events, we need to get the new button states
			// or update the mouse and wheel position as necessary
			if (ev.Type == EventType.MouseInputEvent)
			{
				if (ev.MouseInputEvent.MouseMoved)
				{
					// update mouse positions and wheel information
					mouseDelta.X = mouseDelta.X +
						ev.MousePosition.X - mouseCoords.X;
					mouseDelta.Y = mouseDelta.Y +
						ev.MousePosition.Y - mouseCoords.Y;
					mouseCoords = ev.MousePosition;
				}
				
				if (ev.MouseInputEvent.MouseWheel)
					wheelDelta += ev.MouseWheelDelta;				
					
				// signal that we handled the event
				return true;
			}

			// if we get here, there was no input event
			return false;
		}
		
		// keyboard overloads for key/button states
		public bool IsPressed(KeyCode kc)
		{
			return IsPressed(keyboardStates, (int)kc);
		}

		public bool IsDown(KeyCode kc)
		{
			return IsDown(keyboardStates, (int)kc);
		}

		public bool IsReleased(KeyCode kc)
		{
			return IsReleased(keyboardStates, (int)kc);
		}

		public bool IsUp(KeyCode kc)
		{
			return IsUp(keyboardStates, (int)kc);
		}
		
				
		// main-loop functions to update keystates
		// from pressed to down and from released to up
		// and to start or stop gathering input
		public void OnLoopStart()
		{
			gatheringInput = false;
		}

		public void OnLoopEnd()
		{
			gatheringInput = true;
			
			// "reset" mouse and keyboard states 
			for (int i = 0; i < keyboardStates.Length; ++i)
			{
				if (keyboardStates[i] == KeyState.Released)
					keyboardStates[i] = KeyState.Up;
				if (keyboardStates[i] == KeyState.Pressed)
					keyboardStates[i] = KeyState.Down;
			}
			for (int i = 0; i < mouseButtonStates.Length; ++i)
			{
				if (mouseButtonStates[i] == KeyState.Released)
					mouseButtonStates[i] = KeyState.Up;
				if (mouseButtonStates[i] == KeyState.Pressed)
					mouseButtonStates[i] = KeyState.Down;
			}
			
			// reset mouse deltas
			wheelDelta = 0.0f;
			mouseDelta = new Position2D();
		}
		
		/**
		 * private helper functions
		 */
		// compute the new state based on the previous state
		private KeyState FindNewKeyState(KeyState currState, bool isPressed)
		{
			switch (currState)
			{
				case KeyState.Down:
				case KeyState.Pressed:
					return (isPressed ? KeyState.Down : KeyState.Released);
				case KeyState.Up:
				case KeyState.Released:
					return (isPressed ? KeyState.Pressed : KeyState.Up);
				default:
					throw new Exception();
			}
		}
		
		// takes a literal state and returns a logical state
		private bool IsPressed(KeyState [] states, int idx)
		{	return states[idx] == KeyState.Pressed;	 }
		
		private bool IsDown(KeyState [] states, int idx)
		{
			return states[idx] == KeyState.Pressed ||
				states[idx] == KeyState.Down;
		}
		
		private bool IsReleased(KeyState [] states, int idx)
		{	return states[idx] == KeyState.Released; }
		
		private bool IsUp(KeyState [] states, int idx)
		{
			return states[idx] == KeyState.Released ||
			states[idx] == KeyState.Up;
		}

	}
}
