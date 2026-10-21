using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal static class HelperSweepAndPrune
    {
        internal const float MARGIN = 0.05f;        // base margin of every hull
        internal const float MARGIN_SPEED = 4f;     // + this many steps of the recent speed
        internal const float MARGIN_MAX = 2f;       // upper limit for the margin
        internal const int SHRINK_STEPS = 60;       // hull shrinks back to MARGIN after the hitbox rested for this many steps
        internal const int LAYOUT_STEPS = 240;      // axes and strips are checked every 240 steps
        internal const float STRIP_FACTOR = 4f;     // strip width: 4 x the average hull length
        internal const int MAX_STRIPS = 64;         // upper limit for the number of strips
        internal const float LONG_FACTOR = 4f;      // hulls longer than 4 x the average ...
        internal const int LONG_STRIPS = 4;         // ... or touching more than 4 strips are kept in a separate list
        internal const int BULK_INSERT = 32;        // more new hitboxes than this at once: append and sort the whole list

        private static World _world = null;
        private static int _step = 0;
        private static long _stamp = 0;
        private static int _nextLayoutCheck = 0;

        private static int _axis = 0;                   // sweep axis (0 = x, 1 = y, 2 = z), the lists are sorted along this axis
        private static int _stripAxis = 2;              // axis along which the world is divided into strips
        private static float _stripOrigin = 0f;
        private static float _stripSize = float.MaxValue;
        private static Strip[] _strips = { new Strip() };
        private static int _stripCount = 1;

        // Very long hulls (e.g. a huge floor) would make every query look far back or would be in many strips:
        // they are kept unsorted in a separate small list that is checked by every query.
        private static readonly List<GameObjectHitbox> _long = new();
        private static float _longLimit = float.MaxValue;

        private static int _members = 0;                // number of hitboxes in the broadphase
        private static readonly List<GameObjectHitbox> _new = new();
        private static readonly List<GameObjectHitbox> _all = new();

        // number of hitboxes in the broadphase (for tests)
        internal static int Count { get { return _members; } }





        // clears all lists (world change, shutdown)
        internal static void Reset()
        {
            for (int s = 0; s < _stripCount; s++)
            {
                Strip strip = _strips[s];
                for (int i = 0; i < strip.Count; i++)
                    strip.Items[i]._sapInList = false;
                strip.Clear();
            }
            foreach (GameObjectHitbox hb in _long)
            {
                hb._sapInList = false;
                hb._sapLong = false;
            }
            _long.Clear();
            _new.Clear();
            _members = 0;
            _stripCount = 1;
            _stripSize = float.MaxValue;
            _longLimit = float.MaxValue;
            _nextLayoutCheck = 0;
            _world = null;
            _step++;
        }

        // Once per simulation step (before GameObject.Act()): add new hitboxes, remove old ones, shrink resting hulls.
        // Movements are not handled here but right away in GameObjectHitbox.Update().
        internal static void Update(World w)
        {
            if (w != _world)
            {
                Reset();
                _world = w;
            }
            _step++;

            // 1) visit all hitboxes of the world
            int seen = 0;
            lock (w._gameObjectHitboxes)
            {
                foreach (GameObjectHitbox hb in w._gameObjectHitboxes)
                {
                    if (hb.Owner.IsCollisionObject)
                        seen += Visit(hb);
                }
            }
            if (w._viewSpaceGameObject != null && w._viewSpaceGameObject.IsCollisionObject)
            {
                foreach (GameObjectHitbox hb in w._viewSpaceGameObject._gameObject._colliderModel._hitboxes)
                    seen += Visit(hb);
            }

            // 2) remove hitboxes that no longer belong to the world (object removed, IsCollisionObject = false, other collider model)
            if (seen != _members)
            {
                for (int s = 0; s < _stripCount; s++)
                    _strips[s].RemoveUnseen(_step);
                for (int i = _long.Count - 1; i >= 0; i--)
                {
                    if (_long[i]._sapSeen != _step)
                    {
                        _long[i]._sapInList = false;
                        _long[i]._sapLong = false;
                        _long.RemoveAt(i);
                    }
                }
                _members = seen;
            }

            // 3) add new hitboxes: few one by one, many at once (e.g. when the world starts)
            bool bulk = _new.Count > BULK_INSERT;
            foreach (GameObjectHitbox hb in _new)
                Add(hb, bulk);
            if (bulk)
            {
                for (int s = 0; s < _stripCount; s++)
                    _strips[s].SortIfNeeded();
            }
            _members += _new.Count;
            _new.Clear();

            // 4) every now and then check if other axes or strips would fit better
            if (_step >= _nextLayoutCheck)
            {
                _nextLayoutCheck = _step + LAYOUT_STEPS;
                CheckLayout();
            }
        }

        // returns 1 if the hitbox is already in the broadphase
        private static int Visit(GameObjectHitbox hb)
        {
            if (hb._sapSeen == _step)
                return 0; // duplicate in the world list
            hb._sapSeen = _step;
            if (!hb._sapInList)
            {
                hb._sapInList = true;
                hb._sapLastMove = _step;
                SetHull(hb, MARGIN);
                _new.Add(hb);
                return 0;
            }
            if (hb._sapMargin > MARGIN && _step - hb._sapLastMove > SHRINK_STEPS)
            {
                // resting for a while: shrink the margin back to the base value
                float oldKey = Axis(hb._sapMin);
                SetHull(hb, MARGIN);
                Move(hb, oldKey);
            }
            return 1;
        }


        // If the hitbox is no longer completely inside its hull, it gets a new one right away
        // and moves to its new place in the lists.
        internal static void NoteMoved(GameObjectHitbox hb)
        {
            hb._sapLastMove = _step;
            if (hb._left >= hb._sapMin.X && hb._right <= hb._sapMax.X &&
                hb._low >= hb._sapMin.Y && hb._high <= hb._sapMax.Y &&
                hb._back >= hb._sapMin.Z && hb._front <= hb._sapMax.Z)
                return; // still inside its hull: nothing to do

            // margin depends on the recent speed: distance since the last hull divided by the steps since then
            Vector3 center = new((hb._left + hb._right) * 0.5f, (hb._low + hb._high) * 0.5f, (hb._back + hb._front) * 0.5f);
            float speed = (center - hb._sapCenter).Length / Math.Max(1, _step - hb._sapSince);
            float oldKey = Axis(hb._sapMin);
            SetHull(hb, MathF.Min(MARGIN + MARGIN_SPEED * speed, MARGIN_MAX));
            Move(hb, oldKey);
        }

        private static void SetHull(GameObjectHitbox hb, float margin)
        {
            hb._sapMargin = margin;
            hb._sapMin = new Vector3(hb._left - margin, hb._low - margin, hb._back - margin);
            hb._sapMax = new Vector3(hb._right + margin, hb._high + margin, hb._front + margin);
            hb._sapCenter = new Vector3((hb._left + hb._right) * 0.5f, (hb._low + hb._high) * 0.5f, (hb._back + hb._front) * 0.5f);
            hb._sapSince = _step;
        }

        // ------------------------------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------------------------------

        // collision candidates of an object: hitboxes of other objects whose hull touches a hull of the object
        internal static List<GameObjectHitbox> GetCandidates(GameObject g)
        {
            List<GameObjectHitbox> result = g._collisionCandidates;
            result.Clear();
            if (_world != KWEngine.CurrentWorld)
                return result;

            long stamp = ++_stamp;
            foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                if (hb._sapInList)
                    Query(hb._sapMin, hb._sapMax, g, stamp, result);
            }
            return result;
        }

        // terrain sectors that touch a hull of the object
        internal static List<TerrainSector> GetCandidatesTerrain(GameObject g)
        {
            List<TerrainSector> result = g._collisionCandidatesTerrain;
            result.Clear();
            if (_world != KWEngine.CurrentWorld)
                return result;

            foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                if (hb._sapInList)
                    CollectTerrain(hb, hb._sapMin, hb._sapMax, result);
            }
            return result;
        }

        // hitboxes around the object (for the ray methods of GameObject): like before, all hitboxes
        // that are at most 2 x KWEngine.SweepAndPruneTolerance away from a hitbox of the object
        internal static List<GameObjectHitbox> QueryNearby(GameObject g, List<GameObjectHitbox> result)
        {
            return QueryAround(g, Vector3.Zero, 2f * KWEngine.SweepAndPruneTolerance, result);
        }

        // hitboxes that could touch the hitboxes of the object when moved by offset (for GetIntersections(offset))
        internal static List<GameObjectHitbox> QueryShifted(GameObject g, Vector3 offset, List<GameObjectHitbox> result)
        {
            return QueryAround(g, offset, 0.01f, result);
        }

        private static List<GameObjectHitbox> QueryAround(GameObject g, Vector3 offset, float margin, List<GameObjectHitbox> result)
        {
            result.Clear();
            if (_world != KWEngine.CurrentWorld || !g.IsCollisionObject || g._colliderModel._hitboxes.Count == 0)
                return result;

            Vector3 min = new(float.MaxValue), max = new(float.MinValue);
            foreach (GameObjectHitbox hb in g._colliderModel._hitboxes)
            {
                min = Vector3.ComponentMin(min, new Vector3(hb._left, hb._low, hb._back));
                max = Vector3.ComponentMax(max, new Vector3(hb._right, hb._high, hb._front));
            }
            Query(min + offset - new Vector3(margin), max + offset + new Vector3(margin), g, ++_stamp, result);
            return result;
        }

        // all hitboxes of other objects whose hull touches the given area (each one only once, see stamp)
        private static void Query(Vector3 min, Vector3 max, GameObject skipOwner, long stamp, List<GameObjectHitbox> result)
        {
            StripRange(min, max, out int s0, out int s1);
            for (int s = s0; s <= s1; s++)
                _strips[s].Query(min, max, skipOwner, stamp, result);

            foreach (GameObjectHitbox hb in _long)
            {
                if (hb.Owner != skipOwner && hb._sapStamp != stamp && Overlaps(min, max, hb._sapMin, hb._sapMax))
                {
                    hb._sapStamp = stamp;
                    result.Add(hb);
                }
            }
        }






        // puts a new hitbox into the matching strips (or into the list of long hulls)
        private static void Add(GameObjectHitbox hb, bool append)
        {
            StripRange(hb._sapMin, hb._sapMax, out int s0, out int s1);
            if (IsLong(hb, s0, s1))
            {
                hb._sapLong = true;
                _long.Add(hb);
                return;
            }
            hb._sapLong = false;
            hb._sapStrip0 = s0;
            hb._sapStrip1 = s1;
            for (int s = s0; s <= s1; s++)
            {
                if (append)
                    _strips[s].Append(hb);
                else
                    _strips[s].Insert(hb);
            }
        }

        // the hull has changed: move to the new place in the strips, change strips if necessary
        private static void Move(GameObjectHitbox hb, float oldKey)
        {
            if (hb._sapLong)
                return; // the list of long hulls is not sorted

            StripRange(hb._sapMin, hb._sapMax, out int n0, out int n1);
            int o0 = hb._sapStrip0, o1 = hb._sapStrip1;
            bool isLong = IsLong(hb, n0, n1);
            for (int s = o0; s <= o1; s++)
            {
                if (isLong || s < n0 || s > n1)
                    _strips[s].Remove(hb, oldKey);      // left this strip
                else
                    _strips[s].Reposition(hb, oldKey);  // stayed in this strip
            }
            if (isLong)
            {
                hb._sapLong = true;
                _long.Add(hb);
                return;
            }
            for (int s = n0; s <= n1; s++)
            {
                if (s < o0 || s > o1)
                    _strips[s].Insert(hb);              // entered this strip
            }
            hb._sapStrip0 = n0;
            hb._sapStrip1 = n1;
        }

        private static bool IsLong(GameObjectHitbox hb, int s0, int s1)
        {
            return Axis(hb._sapMax) - Axis(hb._sapMin) > _longLimit || s1 - s0 + 1 > LONG_STRIPS;
        }

        private static void StripRange(Vector3 min, Vector3 max, out int s0, out int s1)
        {
            s0 = StripOf(AxisOf(min, _stripAxis));
            s1 = StripOf(AxisOf(max, _stripAxis));
        }

        private static int StripOf(float value)
        {
            float f = (value - _stripOrigin) / _stripSize;
            if (!(f >= 0f))
                return 0; // also NaN
            if (f >= _stripCount)
                return _stripCount - 1;
            return (int)f;
        }

        private static void CheckLayout()
        {
            // collect every hitbox once (a hitbox can be in more than one strip)
            _all.Clear();
            long stamp = ++_stamp;
            for (int s = 0; s < _stripCount; s++)
            {
                Strip strip = _strips[s];
                for (int i = 0; i < strip.Count; i++)
                {
                    GameObjectHitbox hb = strip.Items[i];
                    if (hb._sapStamp != stamp)
                    {
                        hb._sapStamp = stamp;
                        _all.Add(hb);
                    }
                }
            }
            _all.AddRange(_long);
            if (_all.Count == 0)
                return;

            Vector3 sum = Vector3.Zero, sumSq = Vector3.Zero, lengthSum = Vector3.Zero;
            Vector3 lo = new(float.MaxValue), hi = new(float.MinValue);
            foreach (GameObjectHitbox hb in _all)
            {
                Vector3 c = (hb._sapMin + hb._sapMax) * 0.5f;
                sum += c;
                sumSq += c * c;
                lengthSum += hb._sapMax - hb._sapMin;
                lo = Vector3.ComponentMin(lo, c);
                hi = Vector3.ComponentMax(hi, c);
            }
            int n = _all.Count;
            Vector3 variance = sumSq / n - (sum / n) * (sum / n);
            int axis = variance.X >= variance.Y && variance.X >= variance.Z ? 0 : (variance.Y >= variance.Z ? 1 : 2);
            int other1 = axis == 0 ? 1 : 0, other2 = axis == 2 ? 1 : 2;
            int stripAxis = AxisOf(variance, other1) >= AxisOf(variance, other2) ? other1 : other2;

            float longLimit = MathF.Max(LONG_FACTOR * AxisOf(lengthSum, axis) / n, 0.01f);
            float extent = AxisOf(hi, stripAxis) - AxisOf(lo, stripAxis);
            float stripSize = MathF.Max(STRIP_FACTOR * AxisOf(lengthSum, stripAxis) / n, extent / MAX_STRIPS);
            stripSize = MathF.Max(stripSize, 0.01f);
            int stripCount = Math.Clamp((int)MathF.Ceiling(extent / stripSize) + 1, 1, MAX_STRIPS);
            float origin = AxisOf(lo, stripAxis) - stripSize * 0.5f;
            if (!float.IsFinite(stripSize) || !float.IsFinite(origin))
            {
                stripCount = 1;
                stripSize = float.MaxValue;
                origin = 0f;
            }

            bool rebuild = axis != _axis || stripAxis != _stripAxis
                || stripCount != _stripCount && (stripCount > _stripCount + 1 || stripCount < _stripCount - 1)
                || stripSize < _stripSize * 0.5f || stripSize > _stripSize * 2f
                || MathF.Abs(origin - _stripOrigin) > _stripSize
                || MathF.Abs(longLimit - _longLimit) > 0.5f * _longLimit;
            if (!rebuild)
            {
                for (int s = 0; s < _stripCount; s++)
                    _strips[s].RecomputeMaxLength();
                return;
            }

            // new layout: sort everything in again
            for (int s = 0; s < _stripCount; s++)
                _strips[s].Clear();
            _long.Clear();
            _axis = axis;
            _stripAxis = stripAxis;
            _stripSize = stripSize;
            _stripOrigin = origin;
            _stripCount = stripCount;
            _longLimit = longLimit;
            if (_strips.Length < _stripCount)
            {
                int old = _strips.Length;
                Array.Resize(ref _strips, MAX_STRIPS);
                for (int s = old; s < _strips.Length; s++)
                    _strips[s] = new Strip();
            }
            foreach (GameObjectHitbox hb in _all)
                Add(hb, true);
            for (int s = 0; s < _stripCount; s++)
                _strips[s].SortIfNeeded();
            _all.Clear();
        }

        private static float Axis(Vector3 v)
        {
            return _axis == 0 ? v.X : (_axis == 1 ? v.Y : v.Z);
        }

        private static float AxisOf(Vector3 v, int axis)
        {
            return axis == 0 ? v.X : (axis == 1 ? v.Y : v.Z);
        }

        private static bool Overlaps(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            return aMin.X <= bMax.X && aMax.X >= bMin.X && aMin.Y <= bMax.Y && aMax.Y >= bMin.Y && aMin.Z <= bMax.Z && aMax.Z >= bMin.Z;
        }

        private sealed class Strip
        {
            public GameObjectHitbox[] Items = new GameObjectHitbox[0];
            public float[] Keys = new float[0];         // start of the hull on the sweep axis (the list is sorted by this)
            public Vector3[] Min = new Vector3[0];      // hulls
            public Vector3[] Max = new Vector3[0];
            public int Count = 0;
            public float MaxLength = 0f;                // longest hull on the sweep axis: this is how far a query has to look back
            private bool _needsSort = false;

            public void Clear()
            {
                Array.Clear(Items, 0, Count);
                Count = 0;
                MaxLength = 0f;
                _needsSort = false;
            }

            // the actual sweep
            public void Query(Vector3 min, Vector3 max, GameObject skipOwner, long stamp, List<GameObjectHitbox> result)
            {
                // A hull that starts before the area can be at most MaxLength long: so the query starts at
                // (start of the area - MaxLength) and stops as soon as a hull starts behind the area.
                float to = Axis(max);
                for (int i = LowerBound(Axis(min) - MaxLength); i < Count; i++)
                {
                    if (Keys[i] > to)
                        break;
                    if (!Overlaps(min, max, Min[i], Max[i]))
                        continue;
                    GameObjectHitbox hb = Items[i];
                    if (hb.Owner != skipOwner && hb._sapStamp != stamp)
                    {
                        hb._sapStamp = stamp;
                        result.Add(hb);
                    }
                }
            }

            // insert at the right place (binary search, then move the rest one place back)
            public void Insert(GameObjectHitbox hb)
            {
                EnsureCapacity(Count + 1);
                int pos = LowerBound(Axis(hb._sapMin));
                int move = Count - pos;
                Array.Copy(Items, pos, Items, pos + 1, move);
                Array.Copy(Keys, pos, Keys, pos + 1, move);
                Array.Copy(Min, pos, Min, pos + 1, move);
                Array.Copy(Max, pos, Max, pos + 1, move);
                Place(pos, hb);
                Count++;
            }

            // append without sorting (SortIfNeeded() has to follow)
            public void Append(GameObjectHitbox hb)
            {
                EnsureCapacity(Count + 1);
                Place(Count, hb);
                Count++;
                _needsSort = true;
            }

            public void Remove(GameObjectHitbox hb, float oldKey)
            {
                int i = IndexOf(hb, oldKey);
                if (i < 0)
                    return;
                int move = Count - i - 1;
                Array.Copy(Items, i + 1, Items, i, move);
                Array.Copy(Keys, i + 1, Keys, i, move);
                Array.Copy(Min, i + 1, Min, i, move);
                Array.Copy(Max, i + 1, Max, i, move);
                Count--;
                Items[Count] = null;
            }

            // the hull has changed: move the entry left or right until the order is correct again
            public void Reposition(GameObjectHitbox hb, float oldKey)
            {
                int i = IndexOf(hb, oldKey);
                if (i < 0)
                    return;
                float key = Axis(hb._sapMin);
                while (i > 0 && Keys[i - 1] > key)
                {
                    Copy(i - 1, i);
                    i--;
                }
                while (i < Count - 1 && Keys[i + 1] < key)
                {
                    Copy(i + 1, i);
                    i++;
                }
                Place(i, hb);
            }

            public void RemoveUnseen(int step)
            {
                int write = 0;
                for (int i = 0; i < Count; i++)
                {
                    GameObjectHitbox hb = Items[i];
                    if (hb._sapSeen != step)
                    {
                        hb._sapInList = false;
                        continue;
                    }
                    Copy(i, write);
                    write++;
                }
                Array.Clear(Items, write, Count - write);
                Count = write;
            }

            public void SortIfNeeded()
            {
                if (!_needsSort)
                    return;
                _needsSort = false;
                for (int i = 0; i < Count; i++)
                    Keys[i] = Axis(Items[i]._sapMin);
                Array.Sort(Keys, Items, 0, Count);
                for (int i = 0; i < Count; i++)
                {
                    Min[i] = Items[i]._sapMin;
                    Max[i] = Items[i]._sapMax;
                }
            }

            public void RecomputeMaxLength()
            {
                MaxLength = 0f;
                for (int i = 0; i < Count; i++)
                    MaxLength = MathF.Max(MaxLength, Axis(Max[i]) - Keys[i]);
            }

            // first index whose key is >= key
            private int LowerBound(float key)
            {
                int lo = 0, hi = Count;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (Keys[mid] < key)
                        lo = mid + 1;
                    else
                        hi = mid;
                }
                return lo;
            }

            private int IndexOf(GameObjectHitbox hb, float key)
            {
                for (int i = LowerBound(key); i < Count && Keys[i] == key; i++)
                {
                    if (Items[i] == hb)
                        return i;
                }
                return Array.IndexOf(Items, hb, 0, Count); // should not happen
            }

            private void Place(int i, GameObjectHitbox hb)
            {
                Items[i] = hb;
                Min[i] = hb._sapMin;
                Max[i] = hb._sapMax;
                Keys[i] = Axis(hb._sapMin);
                MaxLength = MathF.Max(MaxLength, Axis(hb._sapMax) - Keys[i]);
            }

            private void Copy(int from, int to)
            {
                Items[to] = Items[from];
                Keys[to] = Keys[from];
                Min[to] = Min[from];
                Max[to] = Max[from];
            }

            private void EnsureCapacity(int n)
            {
                if (Items.Length >= n)
                    return;
                int capacity = Math.Max(16, n * 2);
                Array.Resize(ref Items, capacity);
                Array.Resize(ref Keys, capacity);
                Array.Resize(ref Min, capacity);
                Array.Resize(ref Max, capacity);
            }
        }

        private static void CollectTerrain(GameObjectHitbox hb, Vector3 min, Vector3 max, List<TerrainSector> result)
        {
            if (hb._colliderType == ColliderType.PlaneCollider || hb.Owner.ID <= 0)
                return;

            lock (_world._terrainObjects)
            {
                foreach (TerrainObject t in _world._terrainObjects)
                {
                    if (!t.IsCollisionObject)
                        continue;
                    TerrainObjectHitbox thb = t._hitboxes[0];
                    if (!HelperIntersection.CheckAABBCollision(min.X, max.X, min.Y, max.Y, min.Z, max.Z,
                            thb._left, thb._right, thb._low, thb._high, thb._back, thb._front))
                        continue;

                    float px = t._stateCurrent._position.X;
                    float pz = t._stateCurrent._position.Z;
                    foreach (TerrainSectorCoarseUltra sectorUltra in t._sectorMapCoarseUltra)
                    {
                        if (!HelperIntersection.CheckAABBCollision(sectorUltra.Left + px, sectorUltra.Right + px, sectorUltra.Back + pz, sectorUltra.Front + pz, min.X, max.X, min.Z, max.Z))
                            continue;
                        foreach (TerrainSectorCoarse sectorCoarse in sectorUltra.SectorsCoarse)
                        {
                            if (!HelperIntersection.CheckAABBCollision(sectorCoarse.Left + px, sectorCoarse.Right + px, sectorCoarse.Back + pz, sectorCoarse.Front + pz, min.X, max.X, min.Z, max.Z))
                                continue;
                            foreach (TerrainSector sector in sectorCoarse.Sectors)
                            {
                                if (HelperIntersection.CheckAABBCollision(sector.Left + px, sector.Right + px, sector.Back + pz, sector.Front + pz, min.X, max.X, min.Z, max.Z)
                                    && !result.Contains(sector))
                                {
                                    result.Add(sector);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
