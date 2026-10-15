using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;


namespace KWEngine3TestProject.Classes.WorldPlatformRayTest
{
    internal abstract class EnemyBase : GameObject
    {
        public enum MovementState
        {
            Standing,
            Moving
        }
        public enum ActionState
        {
            Idle,
            //Taunting,
            Alerted,
            Offense,
            AttackDecision,
            Cooldown
        }

        private int _health;
        [KWDebug("Alert factor")]
        private float _alertFactor;
        private float _motionVertical;
        private MovementState _state;
        [KWDebug("Action state")]
        private ActionState _stateAction;

        protected EnemyBase()
        {
            _health = 100;
            _alertFactor = 0f;
            _motionVertical = 0f;
            _stateAction = ActionState.Idle;
            _state = MovementState.Standing;
        }

        public virtual void Dispose()
        {

        }

        protected void SetAlertFactor(float alertFactor)
        {
            _alertFactor = Math.Clamp(alertFactor, 0f, 2f);
        }

        public float GetAlertFactor()
        {
            return _alertFactor;
        }

        protected void SetHealth(int health)
        {
            _health = Math.Clamp(health, 0, 100);
        }

        public int GetHealth()
        {
            return _health;
        }

        protected void SetMovementState(MovementState state)
        {
            _state = state;
        }

        public MovementState GetState()
        {
            return _state;
        }

        protected void SetActionState(ActionState state)
        {
            _stateAction = state;
        }

        public ActionState GetActionState()
        {
            return _stateAction;
        }

        protected void HandleGroundCollision()
        {
            float distanceNearest = float.MaxValue;
            float yPositionNearest = Position.Y;
            bool groundFound = false;


            RayTerrainIntersectionSet rti = RaytraceTerrainBelowPositionExt(Position, RayMode.FourRaysY, 1f);
            if (rti.IsValid && rti.DistanceAvg <= 1.0f)
            {
                distanceNearest = rti.DistanceAvg;
                groundFound = true;
                yPositionNearest = rti.IntersectionPointAvg.Y;
            }

            if (_motionVertical <= 0f)
            {
                RayIntersectionExtSet rti2 = RaytraceObjectsBelowPosition(RayMode.FourRaysY, 0.5f, -0.5f, 0.01f, typeof(GameObject));
                if (rti2.IsValid && Vector3.Dot(rti2.SurfaceNormalNearest, Vector3.UnitY) > 0.5f)
                {
                    if (groundFound)
                    {
                        if (rti2.DistanceMin < distanceNearest)
                        {
                            distanceNearest = rti2.DistanceMin;
                            yPositionNearest = rti2.IntersectionPointNearest.Y;
                        }
                    }
                    else
                    {
                        distanceNearest = rti2.DistanceMin;
                        groundFound = true;
                        yPositionNearest = rti2.IntersectionPointNearest.Y;
                    }
                }
            }

            SetPositionY(yPositionNearest);
            /*
            if (groundFound && distanceNearest < 0.01f)
            {
                
                if (_state == PrimaryState.InAir)
                {
                    _state = PrimaryState.OnGround;
                    _motionVertical = 0f;
                }
            }
            else
            {
                if (_state == PrimaryState.OnGround)
                {
                    _state = PrimaryState.InAir;
                    _motionVertical = 0f;
                }
            }
            */
        }
    }
}
