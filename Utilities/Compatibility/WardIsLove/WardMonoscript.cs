using System;
using UnityEngine;

namespace CraftyBoxes.Compatibility.WardIsLove
{
    public class WardMonoscript : WILCompat
    {
        public static Type ClassType()
        {
            return Type.GetType("WardIsLove.Util.WardMonoscript, WardIsLove")!;
        }

        public static bool CheckInWardMonoscript(Vector3 point, bool flash = false)
        {
            return InvokeMethod<bool>(ClassType(), null!, "CheckInWardMonoscript", new object[] { point, flash });
        }

        public static bool CheckAccess(Vector3 point, float radius = 0.0f, bool flash = true, bool wardCheck = false)
        {
            return InvokeMethod<bool>(ClassType(), null!, "CheckAccess",
                new object[] { point, radius, flash, wardCheck });
        }

        public static bool InsideWard(Vector3 pos)
        {
            return WardIsLovePlugin.WardEnabled()!.Value && CheckInWardMonoscript(pos);
        }
    }
}