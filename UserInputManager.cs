using System;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 
namespace Ymfas
{
    public class UserInputManager
    {
        InputSystem input;
        EventManager m;
        int playerID;

        public const int POSITIVE = 24;
        public const int NEGATIVE = -24;
        public const int AUTOCORRECT = -8;
        public const int FULL = 128;

        public UserInputManager(InputSystem _input, EventManager _m, int _playerID)
	    {
            m = _m;
            playerID = _playerID;

            // initialize the input system
			input = _input;
	    }

        public void PollInputs()
        {
            int pitch = 0;
			int roll = 0;
			int yaw = 0;
			int thrust = 0;

            input.Update();

            if (input.IsDown(Key.E))
                pitch += POSITIVE;

            if (input.IsDown(Key.D))
                pitch += NEGATIVE;

            if (input.IsDown(Key.F))
                yaw += POSITIVE;

            if (input.IsDown(Key.S))
                yaw += NEGATIVE;

            if (input.IsDown(Key.R))
                roll += POSITIVE;

            if (input.IsDown(Key.W))
                roll += NEGATIVE;

			if (input.IsDown(Key.Space))
			{
				thrust = FULL;
			}

            if (input.IsDown(Key.A))
            {
                pitch = AUTOCORRECT;
                roll = AUTOCORRECT;
                yaw = AUTOCORRECT;
            }

            m.SendEvent(new ShipControlStatus(thrust, pitch, roll, yaw, playerID));
        }
    }
}
