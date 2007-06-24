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
        private int mouseWheel, wheelDelta;

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
            mouseWheel = wheelDelta = 0;

            // initially gathering input
            gatheringInput = true;
        }

        public bool OnEvent(Event ev)
        {
            // only process events when not in
            // the main input loop
            if (!gatheringInput)
                return false;

            if (ev.Type == EventType.KeyInputEvent)
            {
                KeyState newKeyState = KeyState.Up;
                // what the new state will be depends on the event 
                // as well as what the current state is
                switch (keyboardStates[(int)ev.KeyCode])
                {
                    case KeyState.Down:
                    case KeyState.Pressed:
                        if (ev.KeyPressedDown)
                            newKeyState = KeyState.Down;
                        else
                            newKeyState = KeyState.Released;
                        break;
                    case KeyState.Up:
                    case KeyState.Released:
                        if (ev.KeyPressedDown)
                            newKeyState = KeyState.Pressed;
                        else
                            newKeyState = KeyState.Up;
                        break;
                    default:
                        break;
                }

                // assign the new key state
                keyboardStates[(int)ev.KeyCode] = newKeyState;

                // signal that we handled the event
                return true;
            }

            // if we get here, there was no input event
            return false;
        }

        public bool IsKeyPressed(KeyCode kc)
        {
            return keyboardStates[(int)kc] == KeyState.Pressed;
        }

        public bool IsKeyDown(KeyCode kc)
        {
            return keyboardStates[(int)kc] == KeyState.Pressed ||
                keyboardStates[(int)kc] == KeyState.Down;
        }

        public bool IsKeyReleased(KeyCode kc)
        {
            return keyboardStates[(int)kc] == KeyState.Released;
        }

        public bool IsKeyUp(KeyCode kc)
        {
            return keyboardStates[(int)kc] == KeyState.Released ||
                keyboardStates[(int)kc] == KeyState.Up;
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
        }
    }
}
