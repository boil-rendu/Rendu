namespace RD.Core
{
    /// <summary>无参 <see cref="ASingleton"/> 的 Awake 钩子，由 <see cref="World.AddSingleton{T}()"/> 在注册时调用。</summary>
    public interface ISingletonAwake
    {
        void Awake();
    }

    /// <summary>1 参 <see cref="ASingleton"/> 的 Awake 钩子，由 <see cref="World.AddSingleton{T,A}(A)"/> 在注册时调用。</summary>
    public interface ISingletonAwake<A>
    {
        void Awake(A a);
    }

    /// <summary>2 参 <see cref="ASingleton"/> 的 Awake 钩子，由 <see cref="World.AddSingleton{T,A,B}(A,B)"/> 在注册时调用。</summary>
    public interface ISingletonAwake<A, B>
    {
        void Awake(A a, B b);
    }

    /// <summary>3 参 <see cref="ASingleton"/> 的 Awake 钩子，由 <see cref="World.AddSingleton{T,A,B,C}(A,B,C)"/> 在注册时调用。</summary>
    public interface ISingletonAwake<A, B, C>
    {
        void Awake(A a, B b, C c);
    }
}
