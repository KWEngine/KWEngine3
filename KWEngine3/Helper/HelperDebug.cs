using KWEngine3.GameObjects;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Helper
{
    internal static class HelperDebug
    {
        internal static readonly Dictionary<Type, List<MemberInfo>> TypesWithDebugAttribute = new();
        // timer queries as a ring over QUERY_FRAMES frames: a result is read QUERY_FRAMES - 1 frames later, so the CPU never waits for the GPU
        internal const int QUERY_FRAMES = 4;
        internal static Dictionary<RenderType, int[]> _renderTimesIDDict = new();
        internal static Dictionary<RenderType, bool[]> _renderTimesIssuedDict = new();
        internal static int _queryFrame = 0; // current ring slot (0 .. QUERY_FRAMES - 1)
        internal static Dictionary<RenderType, List<long>> _renderTimesDict = new();
        internal static Dictionary<RenderType, double> _renderTimesAvgDict = new();
        internal static float _glQueryTimestampLastReset = 0;
        internal static List<float> _cpuTimes = new();
        internal static float _cpuTimeAvg = 0f;
        internal static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        internal static bool HasDebugFields(GameObject g)
        {
            return g != null && TypesWithDebugAttribute.ContainsKey(g.GetType());
        }

        internal static List<MemberInfo> GetKWDebugMembers(GameObject g)
        {
            return TypesWithDebugAttribute[g.GetType()];
        }

        internal static void InitDebugRegistry()
        {
            IEnumerable<Type> allTypes = Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(GameObject).IsAssignableFrom(t) || typeof(World).IsAssignableFrom(t));
            
            foreach (Type type in allTypes)
            {
                List<MemberInfo> mInfo = new();
                List<FieldInfo> fieldsWithAttribute = GetFieldsInHierarchy(type, _bindingFlags)
                    .Where(f => f.IsDefined(typeof(KWDebugAttribute), true))
                    .ToList();
                foreach(FieldInfo f in fieldsWithAttribute)
                {
                    mInfo.Add(f);
                }

                List<PropertyInfo> propertiesWithAttribute = GetPropertiesInHierarchy(type, _bindingFlags)
                    .Where(f => f.IsDefined(typeof(KWDebugAttribute), true))
                    .ToList();

                foreach (PropertyInfo p in propertiesWithAttribute)
                {
                    mInfo.Add(p);
                }

                if (mInfo.Count > 0 && !TypesWithDebugAttribute.ContainsKey(type))
                {
                    TypesWithDebugAttribute.Add(type, mInfo);
                }
            }
        }

        internal static IEnumerable<FieldInfo> GetFieldsInHierarchy(Type type, BindingFlags flags)
        {
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                FieldInfo[] fields = currentType.GetFields(flags);

                foreach (var field in fields)
                {
                    if (field.DeclaringType == currentType)
                    {
                        yield return field;
                    }
                }



                currentType = currentType.BaseType;
            }
        }

        internal static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(Type type, BindingFlags flags)
        {
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                PropertyInfo[] properties = currentType.GetProperties(flags);

                foreach (var prop in properties)
                {
                    if (prop.DeclaringType == currentType)
                    {
                        yield return prop;
                    }
                }

                currentType = currentType.BaseType;
            }
        }

        internal static void Init()
        {
            InitQueries(RenderType.Deferred);
            InitQueries(RenderType.Lighting);
            InitQueries(RenderType.ShadowMapping);
            InitQueries(RenderType.SSAO);
            InitQueries(RenderType.Forward);
            InitQueries(RenderType.HUD);
            InitQueries(RenderType.PostProcessing);
            InitQueries(RenderType.Fog);

            _renderTimesDict[RenderType.Deferred] = new List<long>();
            _renderTimesDict[RenderType.Lighting] = new List<long>();
            _renderTimesDict[RenderType.ShadowMapping] = new List<long>();
            _renderTimesDict[RenderType.SSAO] = new List<long>();
            _renderTimesDict[RenderType.Forward] = new List<long>();
            _renderTimesDict[RenderType.HUD] = new List<long>();
            _renderTimesDict[RenderType.PostProcessing] = new List<long>();
            _renderTimesDict[RenderType.Fog] = new List<long>();

            _renderTimesAvgDict[RenderType.Deferred] = 0;
            _renderTimesAvgDict[RenderType.Lighting] = 0;
            _renderTimesAvgDict[RenderType.ShadowMapping] = 0;
            _renderTimesAvgDict[RenderType.SSAO] = 0;
            _renderTimesAvgDict[RenderType.Forward] = 0;
            _renderTimesAvgDict[RenderType.HUD] = 0;
            _renderTimesAvgDict[RenderType.PostProcessing] = 0;
            _renderTimesAvgDict[RenderType.Fog] = 0;

            InitDebugRegistry();
        }

        private static void InitQueries(RenderType type)
        {
            int[] ids = new int[QUERY_FRAMES];
            GL.GenQueries(QUERY_FRAMES, ids);
            _renderTimesIDDict[type] = ids;
            _renderTimesIssuedDict[type] = new bool[QUERY_FRAMES];
        }

        internal static void ClearTimeDicts()
        {
            if(KWEngine.DebugPerformanceEnabled)
            {
                foreach (var kvpair in _renderTimesDict)
                {
                    _renderTimesDict[kvpair.Key].Clear();
                }
                foreach (var kvpair in _renderTimesAvgDict)
                {
                    _renderTimesAvgDict[kvpair.Key] = 0;
                }
                _cpuTimes.Clear();
                _cpuTimeAvg = 0f;
            }
        }

        internal static void StartTimeQuery(RenderType type)
        {
            if(KWEngine.DebugPerformanceEnabled)
                GL.BeginQuery(QueryTarget.TimeElapsed, _renderTimesIDDict[type][_queryFrame]);
        }

        internal static void StopTimeQuery(RenderType type)
        {
            if (KWEngine.DebugPerformanceEnabled)
            {
                GL.EndQuery(QueryTarget.TimeElapsed);
                int slot = _queryFrame;
                bool[] issued = _renderTimesIssuedDict[type];
                issued[slot] = true;

                // read the oldest query (it gets reused next frame); it ended QUERY_FRAMES - 1 frames ago, so its result is ready
                int oldest = (slot + 1) % QUERY_FRAMES;
                if (issued[oldest])
                {
                    GL.GetQueryObject(_renderTimesIDDict[type][oldest], GetQueryObjectParam.QueryResult, out long drawcalltime);
                    _renderTimesDict[type].Add(drawcalltime);
                    issued[oldest] = false;
                }
            }
        }

        internal static void UpdateTimesAVG()
        {
            _queryFrame = (_queryFrame + 1) % QUERY_FRAMES;
            if (KWEngine.DebugPerformanceEnabled && KWEngine.ApplicationTime - _glQueryTimestampLastReset > 1)
            {
                _glQueryTimestampLastReset = KWEngine.ApplicationTime;
                foreach(var kvpair in _renderTimesDict)
                {
                    List<long> times = kvpair.Value;
                    _renderTimesAvgDict[kvpair.Key] = times.Count > 0 ? times.Average() : 0.0; // no samples yet in the first frames
                    times.Clear();
                }
                _cpuTimeAvg = _cpuTimes.Count > 0 ? _cpuTimes.Average() : 0f;
            }
        }
    }
}
