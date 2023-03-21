﻿using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldFirstPersonView
{
    public class ViewSpaceWeapon : ViewSpaceGameObject
    {
        public override void Act()
        {
            UpdatePosition();
            SetAnimationID(0);
            SetAnimationPercentage(0.9f);
        }
    }
}
