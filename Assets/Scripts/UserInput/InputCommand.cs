using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UserInput
{
    public class InputCommand
    {
       
        public bool DOWN { get; private set; }
        public bool UP { get; private set; }
        public bool HOLD { get; private set; }
        public Vector3 Position { get; private set; }

        public InputCommand(bool pressDown, bool pressUp, bool holding, Vector3 Pos)
        {
            DOWN = pressDown;
            UP = pressUp;
            HOLD = holding;
            Position = Pos;

        }
    }
}