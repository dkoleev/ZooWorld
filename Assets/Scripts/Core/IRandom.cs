namespace ZooWorld.Core
{
    public interface IRandom
    {
        /// <summary>Both bounds are inclusive.</summary>
        float Range(float min, float max);

        /// <summary><paramref name="max"/> is exclusive.</summary>
        int Range(int min, int max);
    }
}
