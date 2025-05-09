namespace UnityHFSM
{
    public interface ITimer
    {
        float ElapsedTime { get; }
        
        void Reset();
    }
}