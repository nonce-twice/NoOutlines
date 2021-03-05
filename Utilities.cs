using System;
using System.Linq;
using UnhollowerRuntimeLib.XrefScans;
using System.Reflection;
using System.Collections;
using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using VRC;
using Harmony;

namespace NoOutlines
{
    // Large parts of this class comes from DynamicBonesSafety mod by Ben
    // https://github.com/BenjaminZehowlt/DynamicBonesSafety/
    // Thanks to loukylor for pointing me in the right direction
    static class Utilities
    {
            public static bool checkXref(this MethodBase m, string match)
            {
                try
                {
                    return XrefScanner.XrefScan(m).Any(
                        instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null && instance.ReadAsObject().ToString()
                                       .Equals(match, StringComparison.OrdinalIgnoreCase));
                }
                catch { } // ignored

                return false;
            }
            public static bool checkXrefMethodPartial(this MethodBase m, string partialMatch)
            {
                try
                {
                    return XrefScanner.XrefScan(m).Any(
                        instance => instance.Type == XrefType.Method && instance.TryResolve().Name.Contains(partialMatch));
                }
                catch { } // ignored

                return false;
            }

    }
}