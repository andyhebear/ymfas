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

        const sbyte POSITIVE = 127;
        const sbyte NEGATIVE = -127;
        const sbyte AUTOCORRECT = -128;
        const byte FULL = 255;

        public UserInputManager(InputSystem _input, EventManager _m, byte _playerID)
	    {
            m = _m;
            playerID = _playerID;

            // initialize the input system
			input = _input;
	    }

        public void PollInputs()
        {
            sbyte pitch = 0;
            sbyte roll = 0;
            sbyte yaw = 0;
            byte thrust = 0;

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
                thrust = FULL;

            if (input.IsDown(Key.A))
            {
                pitch = AUTOCORRECT;
                roll = AUTOCORRECT;
                yaw = AUTOCORRECT;
            }

            m.SendEvent(new ShipControlStatus(thrust, pitch, roll, yaw, (byte)playerID));
        }
    }
}
