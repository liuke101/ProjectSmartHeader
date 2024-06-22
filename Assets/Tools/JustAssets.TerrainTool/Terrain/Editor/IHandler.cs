namespace JustAssets.TerrainUtility
{
    public interface IHandler<T>
    {
        T[] Selection { get; set; }

        object this[int index] { get; set; }

        bool IsValid(out string reason);

        void Execute();
    }
}