namespace RD.Core
{
    public interface IScene
    {
        Fiber Fiber { get; set; }
        int SceneType { get; set; }
    }
}