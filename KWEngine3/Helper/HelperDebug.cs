using KWEngine3.GameObjects;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace KWEngine3.Helper
{
    internal static class HelperDebug
    {
        internal static readonly Dictionary<Type, List<FieldInfo>> TypesWithDebugAttribute = new();
        internal static Dictionary<RenderType, int> _renderTimesIDDict = new();
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

        internal static List<FieldInfo> GetKWDebugFields(GameObject g)
        {
            return TypesWithDebugAttribute[g.GetType()];
        }

        internal static void InitDebugRegistry()
        {
            IEnumerable<Type> allTypes = Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(GameObject).IsAssignableFrom(t) || typeof(World).IsAssignableFrom(t));
           
            foreach (Type type in allTypes)
            {
                var fieldsWithAttribute = GetFieldsInHierarchy(type, _bindingFlags)
                    .Where(f => f.IsDefined(typeof(KWDebugAttribute), true))
                    .ToList();

                if (fieldsWithAttribute.Count > 0 && !TypesWithDebugAttribute.ContainsKey(type))
                {
                    TypesWithDebugAttribute.Add(type, fieldsWithAttribute);
                }
            }
        }

        internal static IEnumerable<FieldInfo> GetFieldsInHierarchy(Type type, BindingFlags flags)
        {
            var currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                var fields = currentType.GetFields(flags);

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

        internal static void Init()
        {
            _renderTimesIDDict[RenderType.Deferred] = GL.GenQuery();
            _renderTimesIDDict[RenderType.Lighting] = GL.GenQuery();
            _renderTimesIDDict[RenderType.ShadowMapping] = GL.GenQuery();
            _renderTimesIDDict[RenderType.SSAO] = GL.GenQuery();
            _renderTimesIDDict[RenderType.Forward] = GL.GenQuery();
            _renderTimesIDDict[RenderType.HUD] = GL.GenQuery();
            _renderTimesIDDict[RenderType.PostProcessing] = GL.GenQuery();

            _renderTimesDict[RenderType.Deferred] = new List<long>();
            _renderTimesDict[RenderType.Lighting] = new List<long>();
            _renderTimesDict[RenderType.ShadowMapping] = new List<long>();
            _renderTimesDict[RenderType.SSAO] = new List<long>();
            _renderTimesDict[RenderType.Forward] = new List<long>();
            _renderTimesDict[RenderType.HUD] = new List<long>();
            _renderTimesDict[RenderType.PostProcessing] = new List<long>();

            _renderTimesAvgDict[RenderType.Deferred] = 0;
            _renderTimesAvgDict[RenderType.Lighting] = 0;
            _renderTimesAvgDict[RenderType.ShadowMapping] = 0;
            _renderTimesAvgDict[RenderType.SSAO] = 0;
            _renderTimesAvgDict[RenderType.Forward] = 0;
            _renderTimesAvgDict[RenderType.HUD] = 0;
            _renderTimesAvgDict[RenderType.PostProcessing] = 0;

            InitDebugRegistry();
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
                GL.BeginQuery(QueryTarget.TimeElapsed, _renderTimesIDDict[type]);
        }

        internal static void StopTimeQuery(RenderType type)
        {
            if (KWEngine.DebugPerformanceEnabled)
            {
                GL.EndQuery(QueryTarget.TimeElapsed);
                GL.GetQueryObject(_renderTimesIDDict[type], GetQueryObjectParam.QueryResult, out long drawcalltime);
                _renderTimesDict[type].Add(drawcalltime);
            }
        }

        internal static void UpdateTimesAVG()
        {
            if (KWEngine.DebugPerformanceEnabled && KWEngine.ApplicationTime - _glQueryTimestampLastReset > 1)
            {
                _glQueryTimestampLastReset = KWEngine.ApplicationTime;
                foreach(var kvpair in _renderTimesDict)
                {
                    _renderTimesAvgDict[kvpair.Key] = _renderTimesDict[kvpair.Key].Average();
                    _renderTimesDict[kvpair.Key].Clear();
                }
                _cpuTimeAvg = _cpuTimes.Average();
            }
        }
    }
}
