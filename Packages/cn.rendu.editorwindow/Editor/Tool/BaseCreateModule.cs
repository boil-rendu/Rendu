namespace RD.EditorWindows.Editor
{
    /// <summary>
    /// 工具模块的最底层基类：只定义 Initialize / OnDestroy 生命周期契约。
    /// 由具体的功能模块继承后实现, 进菜单树的挂 [UIAutoMenu], 不进菜单的(嵌在父模块里) 直接继承。
    /// </summary>
    public abstract class BaseCreateModule
    {
        public virtual void Initialize()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }
}
