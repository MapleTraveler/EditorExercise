using UnityEngine;

namespace UnityHFSM
{
    /// <summary>
    /// 根据 <c>Time.time</c> 计算经过时间的默认计时器。
    /// </summary>
    public class Timer : ITimer
    {
        public float startTime;
        public float ElapsedTime => Time.time - startTime;
        public void Reset()
        {
            startTime = Time.time;
        }

        public static bool operator >(Timer timer, float duration)
        {
            return timer.ElapsedTime > duration;
        }

        public static bool operator <(Timer timer, float duration)
        {
            return timer.ElapsedTime < duration;
        }
        
        public static bool operator >=(Timer timer, float duration)
        {
            return timer.ElapsedTime >= duration;
        }

        public static bool operator <=(Timer timer, float duration)
        {
            return timer.ElapsedTime <= duration;
        }
    }
}