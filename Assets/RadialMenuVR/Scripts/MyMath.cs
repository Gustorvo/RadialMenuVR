using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public static class MyMath
    {
        internal static Vector3 GetAngularVelocity(Quaternion previousRotation, Quaternion currentRotation)
        {
            var q = currentRotation * Quaternion.Inverse(previousRotation);
            // no rotation?
            // You may want to increase this closer to 1 if you want to handle very small rotations.
            // Beware, if it is too close to one your answer will be Nan
            if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
                return new Vector3(0, 0, 0);
            float gain;
            // handle negatives, we could just flip it but this is faster
            if (q.w < 0.0f)
            {
                var angle = Mathf.Acos(-q.w);
                gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            else
            {
                var angle = Mathf.Acos(q.w);
                gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            return new Vector3(q.x * gain, q.y * gain, q.z * gain);
        }
        internal static Vector3 GetAngularVelocity2(Quaternion previousRotation, Quaternion currentRotation)
        {
            Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            angle *= Mathf.Deg2Rad;
            return (1.0f / Time.deltaTime) * angle * axis;
        }
        public static Vector4 ToVector4(Quaternion q) => new Vector4(q.x, q.y, q.z, q.w);
        public static Quaternion FromVector4(Vector4 v)
        {
            Vector4.Normalize(v);
            return new Quaternion(v.x, v.y, v.z, v.w);
        }       
               
        public static bool EqualTo(this Quaternion first, Quaternion second, float tolerance = float.Epsilon)
        {
            return Mathf.Abs(first.x - second.x) <= tolerance && Mathf.Abs(first.y - second.y) <= tolerance && Mathf.Abs(first.z - second.z) <= tolerance && Mathf.Abs(first.w - second.w) <= tolerance;
        }
    }
}
