namespace ZooWorld.Core.Animals
{
    public interface IDiet
    {
        public string Id { get; }
        public bool CanEat(IDiet other);
    }
}