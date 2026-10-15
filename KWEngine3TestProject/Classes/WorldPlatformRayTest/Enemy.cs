using KWEngine3;
using KWEngine3.Exceptions;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Worlds;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Xml.Linq;

namespace KWEngine3TestProject.Classes.WorldPlatformRayTest
{
    internal class Enemy : EnemyBase
    {
        #region AnimationIDs
        private const int ENEMY_ANIM_UNARMED_STAND_IDLE_ID = 1;

        private const int ENEMY_ANIM_UNARMED_STAND_WALK_FORWARD_ID = 2;
        private const int ENEMY_ANIM_UNARMED_STAND_WALK_LEFT_ID = 3;
        private const int ENEMY_ANIM_UNARMED_STAND_WALK_BACKWARD_ID = 4;
        private const int ENEMY_ANIM_UNARMED_STAND_WALK_RIGHT_ID = 5;

        private const int ENEMY_ANIM_UNARMED_STAND_RUN_FORWARD_ID = 6;
        private const int ENEMY_ANIM_UNARMED_STAND_RUN_BACKWARD_ID = 7;

        private const int ENEMY_ANIM_UNARMED_STAND_RUN_JUMP_ID = 8;
        private const int ENEMY_ANIM_UNARMED_STAND_JUMP_ID = 9;

        private const int ENEMY_ANIM_UNARMED_STAND_TO_CROUCH_ID = 10;
        private const int ENEMY_ANIM_UNARMED_CROUCH_IDLE_ID = 11;
        private const int ENEMY_ANIM_UNARMED_CROUCH_TO_STAND_ID = 12;

        private const int ENEMY_ANIM_UNARMED_CROUCH_WALK_FORWARD_ID = 13;
        private const int ENEMY_ANIM_UNARMED_CROUCH_WALK_LEFT_ID = 14;
        private const int ENEMY_ANIM_UNARMED_CROUCH_WALK_BACKWARD_ID = 15;
        private const int ENEMY_ANIM_UNARMED_CROUCH_WALK_RIGHT_ID = 16;

        private const int ENEMY_ANIM_UNARMED_STAND_REACT_DEATH_FRONT_ID = 17;
        private const int ENEMY_ANIM_UNARMED_STAND_REACT_DEATH_LEFT_ID = 18;
        private const int ENEMY_ANIM_UNARMED_STAND_REACT_DEATH_RIGHT_ID = 10;

        private const int ENEMY_ANIM_UNARMED_STAND_EQUIP_UNDERARM_ID = 20;

        private const int ENEMY_ANIM_ARMED_STAND_IDLE_ID = 21;

        private const int ENEMY_ANIM_ARMED_STAND_WALK_FORWARD_ID = 22;
        private const int ENEMY_ANIM_ARMED_STAND_WALK_LEFT_ID = 23;
        private const int ENEMY_ANIM_ARMED_STAND_WALK_BACKWARD_ID = 24;
        private const int ENEMY_ANIM_ARMED_STAND_WALK_RIGHT_ID = 25;

        private const int ENEMY_ANIM_ARMED_STAND_RUN_FORWARD_ID = 26;
        private const int ENEMY_ANIM_ARMED_STAND_RUN_BACKWARD_ID = 27;

        private const int ENEMY_ANIM_ARMED_STAND_JUMP_ID = 28;

        private const int ENEMY_ANIM_ARMED_STAND_TO_CROUCH_ID = 29;
        private const int ENEMY_ANIM_ARMED_CROUCH_IDLE_ID = 30;
        private const int ENEMY_ANIM_ARMED_CROUCH_TO_STAND_ID = 31;

        private const int ENEMY_ANIM_ARMED_CROUCH_WALK_FORWARD_ID = 32;
        private const int ENEMY_ANIM_ARMED_CROUCH_WALK_LEFT_ID = 33;
        private const int ENEMY_ANIM_ARMED_CROUCH_WALK_BACKWARD_ID = 34;
        private const int ENEMY_ANIM_ARMED_CROUCH_WALK_RIGHT_ID = 35;

        private const int ENEMY_ANIM_ARMED_BLOCK_ATTACK_IDLE_ID = 36;
        private const int ENEMY_ANIM_ARMED_REACT_BLOCK_ATTACK_ID = 37;

        private const int ENEMY_ANIM_ARMED_STAND_REACT_HIT_FRONT_ID = 38;
        private const int ENEMY_ANIM_ARMED_STAND_REACT_HIT_LEFT_ID = 39;
        private const int ENEMY_ANIM_ARMED_STAND_REACT_HIT_RIGHT_ID = 40;

        private const int ENEMY_ANIM_ARMED_STAND_HORIZONTAL_ATTACK_ID = 41;
        private const int ENEMY_ANIM_ARMED_STAND_DOWNWARD_ATTACK_ID = 42;
        private const int ENEMY_ANIM_ARMED_STAND_BACKHAND_ATTACK_ID = 43;

        private const int ENEMY_ANIM_ARMED_STAND_360_LOW_ATTACK_ID = 44;
        private const int ENEMY_ANIM_ARMED_STAND_360_HIGH_ATTACK_ID = 45;

        private const int ENEMY_ANIM_ARMED_STAND_COMBO_ATTACK_V1_ID = 46;
        private const int ENEMY_ANIM_ARMED_STAND_COMBO_ATTACK_V2_ID = 47;
        private const int ENEMY_ANIM_ARMED_STAND_COMBO_ATTACK_V3_ID = 48;

        private const int ENEMY_ANIM_ARMED_STAND_RUN_JUMP_ATTACK_ID = 49;

        private const int ENEMY_ANIM_ARMED_STAND_DISARM_UNDERARM_ID = 50;
        #endregion

        private List<RayIntersection> _rayIntersections;
        private float _animationPercentage;
        //private bool _animationJustSwitched;
        [KWDebug("Exhaustion meter")]
        private float _exhaustionMeter;
        private FlowField _flowField;
        private List<Vector3> _patrolPoints;
        private Vector3 _patrolPositionIntermediate;
        private Vector3 _patrolPositionTarget;
        private int _patrolPointIndex;
        private float _movementSpeed;
        private Quaternion _rotation;
        private const float ROTATION_SMOOTHING = 2.5f;

        public Enemy(string name)
            : base()
        {
            Name = name;

            IsCollisionObject = true;
            IsShadowCaster = true;
            SetModel("Brute");
            SetHitboxToCapsule();
            SetHitboxScale(0.5f, 1f, 1f);

            _movementSpeed = 0.009f;
            _rayIntersections = new List<RayIntersection>();
            _animationPercentage = 0f;
            //_animationJustSwitched = false;
            _exhaustionMeter = 0f;

            _patrolPoints = new List<Vector3>();
            _patrolPointIndex = -1;
            _patrolPositionIntermediate = Vector3.Zero;

            _flowField = new FlowField(0, 2.5f, 0, 20, 20, 1f, 5, FlowFieldMode.Box, typeof(Immovable));
            _flowField.SetAllowedDirections(FlowFieldDirections.CardinalAndIntercardinalDirections);
            _flowField.SetCellHitboxScale(1.25f);
            _flowField.Name = "FlowField " + name;
            _flowField.IsVisible = Camera.CAM_DISABLE;

            _rotation = Rotation;

            CurrentWorld.AddFlowField(_flowField);
        }

        public override void Act()
        {
            base.HandleGroundCollision();

            UpdateMovement();

            UpdateFlowField();

            UpdateAnimations();

            UpdateAlertMeter();
        }

        public void AddPatrolPoint(Vector3 p)
        {
            if (_patrolPoints.Contains(p) == false)
                _patrolPoints.Add(p);
        }

        private bool GetNextCellPosition(out Vector3 position)
        {
            position = Vector3.Zero;

            if (_flowField.HasTarget)
            {
                FlowFieldCell currentCell = _flowField.GetCellForWorldPosition(Position, true);
                FlowFieldCell neighbourCell = null;
                if (currentCell.BestDirectionCardinal == CardinalDirection.North)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(0, -1);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.East)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(+1, 0);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.South)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(0, +1);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.West)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(-1, 0);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.NorthEast)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(+1, -1);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.NorthWest)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(-1, -1);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.SouthEast)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(+1, +1);
                    position = neighbourCell.Position;
                }
                else if (currentCell.BestDirectionCardinal == CardinalDirection.SouthWest)
                {
                    neighbourCell = currentCell.GetNeighbourCellAtOffset(-1, +1);
                    position = neighbourCell.Position;
                }
                else
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateMovement()
        {
            Player p = (CurrentWorld as GameWorldPlatformRayTest).GetPlayer();

            float t = 1.0f - MathF.Exp(-ROTATION_SMOOTHING * (1f / 240f));
            float tNear = 1.0f - MathF.Exp(-ROTATION_SMOOTHING * 4f * (1f / 240f));

            if (GetActionState() == ActionState.Idle)
            {
                SetColor(1f, 1f, 1f);
                SetColorEmissive(0f, 0f, 0f, 0f);

                if (GetState() == MovementState.Moving)
                {
                    if (_flowField.HasTarget)
                    {
                        float distanceToTarget = HelperVector.GetDistanceBetweenVectorsXZ(Position, _patrolPositionTarget);

                        if (distanceToTarget <= 0.1f)
                        {
                            _flowField.UnsetTarget();
                            UpdateExhaustion(1f);
                            _patrolPositionIntermediate = Vector3.Zero;
                            SetMovementState(MovementState.Standing);
                        }
                        else if (distanceToTarget > 0.1f && distanceToTarget < 2.0f)
                        {
                            Vector3 dir = _flowField.GetLinearDirectionForPosition(Position);
                            Quaternion newRotation = HelperRotation.GetRotationForPoint(Position, Position + dir);
                            _rotation = Quaternion.Normalize(Quaternion.Slerp(_rotation, newRotation, tNear));
                            SetRotation(_rotation);
                            Move(_movementSpeed);
                        }
                        else
                        {
                            if (_patrolPositionIntermediate != Vector3.Zero)
                            {
                                FlowFieldCell intermediateCell = _flowField.GetCellForWorldPosition(Position);
                                Vector3 dir = intermediateCell.BestDirection;

                                Quaternion newRotation = HelperRotation.GetRotationForPoint(Position, Position + dir);
                                _rotation = Quaternion.Normalize(Quaternion.Slerp(_rotation, newRotation, t));
                                SetRotation(_rotation);
                                Move(_movementSpeed);

                                float distanceToIntermediate = HelperVector.GetDistanceBetweenVectorsXZ(Position, _patrolPositionIntermediate);
                                if (distanceToIntermediate <= 0.25f && GetNextCellPosition(out Vector3 nextIntermediate) == true)
                                {
                                    _patrolPositionIntermediate = nextIntermediate;
                                }
                            }
                            else
                            {
                                FlowFieldCell targetCell = _flowField.GetCellForWorldPosition(_patrolPositionTarget);
                                if (!targetCell.ContainsXZ(this))
                                {
                                    if (GetNextCellPosition(out Vector3 tmp))
                                    {
                                        _patrolPositionIntermediate = tmp;
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //SetState(MovementState.Standing);
                    }
                }
                else
                {
                    UpdateExhaustion(-0.001f);
                    if (_exhaustionMeter == 0f)
                    {
                        if (_patrolPoints.Count > 0)
                        {
                            SetMovementState(MovementState.Moving);
                            _patrolPointIndex = (_patrolPointIndex + 1) % _patrolPoints.Count;
                            _flowField.SetTarget(_patrolPoints[_patrolPointIndex]);
                            _patrolPositionTarget = _patrolPoints[_patrolPointIndex];
                        }
                    }
                }
            }
            else if (GetActionState() == ActionState.Alerted)
            {
                SetColor(0.75f, 0.75f, 0);
                SetColorEmissive(0f, 0f, 0f, 0f);

                if (!_flowField.HasTarget)
                {
                    _flowField.SetTarget(p.Position, true);
                    return;
                }
                else
                {
                    float distanceToTarget = HelperVector.GetDistanceBetweenVectorsXZ(Position, p.Position);
                    if (distanceToTarget < 20f)
                    {
                        Vector3 dir = _flowField.GetBestDirectionForPosition(Position);
                        if (dir != Vector3.Zero)
                        {
                            TurnTowardsXZ(Position + dir);
                            Move(_movementSpeed * 1.25f);
                            UpdateExhaustion(0.0001f);
                        }

                        if (GetAlertFactor() >= 2f)
                        {
                            SetActionState(ActionState.Offense);
                            _flowField.SetTarget(p.Position, true);
                        }
                    }
                    else
                    {
                        SetMovementState(MovementState.Standing);
                        UpdateExhaustion(1f);
                        SetActionState(ActionState.Cooldown);
                    }
                }
            }
            else if (GetActionState() == ActionState.Offense)
            {
                SetColor(1, 0, 0);
                SetColorEmissive(0f, 0f, 0f, 0f);
                _flowField.SetTarget(p.Position, true);
                float distanceToTarget = HelperVector.GetDistanceBetweenVectorsXZ(Position, p.Position);

                if (distanceToTarget < 2.5f)
                {
                    SetActionState(ActionState.AttackDecision);
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_HORIZONTAL_ATTACK_ID);
                }
                else if (distanceToTarget >= 0.25f && distanceToTarget < 2f)
                {
                    TurnTowardsXZ(p.Position);
                    Move(Player.SPEED);
                }
                else if (distanceToTarget >= 2f && distanceToTarget < 20f)
                {
                    Vector3 dir = _flowField.GetBestDirectionForPosition(Position);
                    if (dir != Vector3.Zero)
                    {
                        TurnTowardsXZ(Position + dir);
                        Move(Player.SPEED * 1.001f);
                        UpdateExhaustion(0.0001f);
                    }
                }
                else
                {
                    SetMovementState(MovementState.Standing);
                    UpdateExhaustion(1f);
                    SetActionState(ActionState.Cooldown);
                }


            }
            else if (GetActionState() == ActionState.AttackDecision)
            {
                SetColor(1f, 0f, 0f);
                SetColorEmissive(1f, 0f, 0f, 1f);
                _flowField.SetTarget(p.Position, true);
                float distanceToTarget = HelperVector.GetDistanceBetweenVectorsXZ(Position, p.Position);

                if (distanceToTarget < 4f)
                {
                    TurnTowardsXZ(p.Position);
                    Move(Player.SPEED * 1.005f);
                }
                else
                {
                    SetActionState(ActionState.Offense);
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_RUN_FORWARD_ID);
                }
            }

            List<Intersection> intersections = GetIntersections<Immovable>();
            foreach (Intersection i in intersections)
            {
                MoveOffset(i.MTV);
            }
        }

        private void UpdateFlowField()
        {
            if (_flowField == null) return;
            if (Math.Abs(_flowField.Center.X - Position.X) > 4 || Math.Abs(_flowField.Center.Z - Position.Z) > 4)
                _flowField.SetPosition(MathF.Floor(this.Position.X), MathF.Floor(this.Position.Z));
            _flowField.Update();
        }

        private bool UpdateAnimations(float amount, bool loop)
        {
            bool animationHasEnded = false;

            SetAnimationPercentage(_animationPercentage);

            _animationPercentage += amount;
            if (loop)
            {
                if (_animationPercentage > 1f)
                {
                    _animationPercentage = _animationPercentage - 1f;
                }
                else if (_animationPercentage < 0f)
                {
                    _animationPercentage = 1f + _animationPercentage;
                }
            }
            else
            {
                if (amount < 0)
                {
                    if (_animationPercentage < 0f)
                    {
                        _animationPercentage = 0f;
                        animationHasEnded = true;
                    }
                }
                else
                {
                    if (_animationPercentage > 1f)
                    {
                        _animationPercentage = 1f;
                        animationHasEnded = true;
                    }
                }
            }

            return animationHasEnded;
        }

        private void UpdateExhaustion(float amount)
        {
            _exhaustionMeter += amount;

            if (amount < 0)
            {
                if (_exhaustionMeter < 0f)
                    _exhaustionMeter = 0f;
            }
            else
            {
                if (_exhaustionMeter > 1f)
                    _exhaustionMeter = 1f;
            }

            if (_exhaustionMeter == 1f)
            {
                _animationPercentage = 0f;
            }
        }

        private void UpdateAnimations()
        {
            if (GetState() == MovementState.Standing)
            {
                if (GetActionState() == ActionState.Idle || GetActionState() == ActionState.Alerted)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_IDLE_ID);
                    UpdateAnimations(amount: 0.005f, loop: true);
                }
                else if (GetActionState() == ActionState.Cooldown)
                {
                    if (AnimationID == ENEMY_ANIM_ARMED_STAND_TO_CROUCH_ID)
                    {
                        SetAnimationID(ENEMY_ANIM_ARMED_STAND_TO_CROUCH_ID);
                        if (UpdateAnimations(amount: 0.001f, loop: false))
                        {
                            SetAnimationID(ENEMY_ANIM_ARMED_CROUCH_IDLE_ID);
                            _animationPercentage = 0f;
                        }
                    }
                    else
                    {
                        SetAnimationID(ENEMY_ANIM_ARMED_CROUCH_IDLE_ID);
                        UpdateAnimations(amount: 0.005f, loop: true);
                    }
                }
            }
            else // moving around
            {
                if (GetActionState() == ActionState.Idle)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_WALK_FORWARD_ID);
                    UpdateAnimations(amount: 0.005f, loop: true);

                }
                else if (GetActionState() == ActionState.Alerted)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_WALK_FORWARD_ID);
                    UpdateAnimations(0.005f, loop: true);

                }
                else if (GetActionState() == ActionState.Offense)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_RUN_FORWARD_ID);
                    UpdateAnimations(0.005f, loop: true);
                }
                else if (GetActionState() == ActionState.AttackDecision)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_RUN_JUMP_ATTACK_ID);
                    if (UpdateAnimations(0.00125f, loop: false))
                    {
                        UpdateExhaustion(1f);
                    }
                }
                else if (GetActionState() == ActionState.Cooldown)
                {
                    SetAnimationID(ENEMY_ANIM_ARMED_STAND_IDLE_ID);
                    UpdateAnimations(amount: 0.005f, loop: true);
                }
            }
        }

        private void UpdateAlertMeter()
        {
            // safety check for player object in world:
            Player player = null;
            if (CurrentWorld is GameWorldPlatformRayTest)
            {
                player = (CurrentWorld as GameWorldPlatformRayTest).GetPlayer();

            }
            if (player == null)
            {
                throw new EngineException("Enemy class cannot detect Player instance");
            }

            // if the safety check was successful, check the direction and distance to the player object
            Vector3 enemyToPlayer = player.Center - this.Center;
            float enemyToPlayerDistance = enemyToPlayer.LengthFast;
            Vector3 enemyToPlayerNormalized = Vector3.NormalizeFast(enemyToPlayer);
            float dotEnemyPlayer = Vector3.Dot(LookAtVector, enemyToPlayerNormalized);
            float currentAlertFactor = GetAlertFactor();
            _rayIntersections.Clear();

            if (GetActionState() == ActionState.Offense)
            {

            }
            else if (GetActionState() == ActionState.AttackDecision)
            {

            }
            else if (GetActionState() == ActionState.Cooldown)
            {
                UpdateExhaustion(-0.001f);
                currentAlertFactor -= 0.0001f;
                SetAlertFactor(currentAlertFactor);
            }
            else
            {
                // populate a raycast result list for the current frame. 
                if (enemyToPlayerDistance < 20f)
                {
                    _rayIntersections.AddRange(HelperIntersection.RayTraceObjectsForViewVectorFastest(this.Center, enemyToPlayerNormalized, this, 20f, true, typeof(Player), typeof(Immovable)));
                }

                if (_rayIntersections.Count > 0)
                {
                    if (_rayIntersections[0].Object is Player)
                    {
                        if (currentAlertFactor == 0)
                            currentAlertFactor = 0.001f;
                        currentAlertFactor = currentAlertFactor + 0.002f * MathF.Pow(dotEnemyPlayer, 3f) * (10f / enemyToPlayerDistance);
                        SetAlertFactor(currentAlertFactor);
                    }
                    else
                    {
                        if (GetActionState() == ActionState.Alerted)
                        {
                            if (enemyToPlayerDistance > 20)
                            {
                                currentAlertFactor = currentAlertFactor - 0.0002f;
                                SetAlertFactor(currentAlertFactor);
                            }
                        }
                        else if (GetActionState() == ActionState.Idle)
                        {
                            if (enemyToPlayerDistance > 10)
                            {
                                currentAlertFactor = currentAlertFactor - 0.0002f;
                                SetAlertFactor(currentAlertFactor);
                            }
                        }
                    }
                }
                else
                {
                    currentAlertFactor = currentAlertFactor - 0.0003f;
                    SetAlertFactor(currentAlertFactor);
                }
            }

            if (GetAlertFactor() < 1f)
            {

            }
            else if (GetAlertFactor() >= 1f && GetAlertFactor() < 2f)
            {
                if (GetActionState() < ActionState.Alerted)
                {
                    UpdateExhaustion(-1f);
                    SetMovementState(MovementState.Moving);
                    SetActionState(ActionState.Alerted);
                    _animationPercentage = 0f;
                }
            }
            else
            {
                if (GetActionState() == ActionState.Alerted)
                {
                    SetActionState(ActionState.Offense);
                    SetMovementState(MovementState.Moving);
                    _animationPercentage = 0f;
                }
            }
        }

        public override void Dispose()
        {
            CurrentWorld.RemoveFlowField(_flowField);
        }
    }
}
