public interface IState
{
    void _Update();
    void _FixedUpdate();
    void _LateUpdate();
    void _Enter();
    void _Enter<T>(T msg);
    void _Exit();
}
