﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LEAN
{
    public class OutSocket : Socket
    {
        private void Awake()
        {
            _in = false;
        }
    }
}