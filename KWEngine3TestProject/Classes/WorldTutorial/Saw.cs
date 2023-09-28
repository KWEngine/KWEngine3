using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class Saw : GameObject
    {
        private SawText _text = null;
        private int _health = 100;

        public Saw()
        {
            SetModel("Hazard_Saw");
            SetColorEmissive(1, 1, 1, 0.25f);
            _text = new SawText(this);
            
            UpdateText();
            KWEngine.CurrentWorld.AddTextObject(_text);
            IsCollisionObject = true;
            
        }
        public override void Act()
        {
            //MoveOffset(0, 0, 0.01f);
            AddRotationZ(1);
            _text.SetPosition(Position + new Vector3(0, 0, 0.1f));
            
        }

        private void UpdateText()
        {
            _text.SetText(("" + _health));
        }

        public void TakeDamage(int amount)
        {
            _health = Math.Max(0, _health - amount);
            UpdateText();
            if(_health == 0)
            {
                CurrentWorld.RemoveTextObject(_text);
                CurrentWorld.RemoveGameObject(this);
            }
        }
    }
}
