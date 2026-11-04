namespace EasyFramework
{
    public abstract partial class Controller<T> where T : Controller<T>, new()
    {
        public static T Instance => ControllerManager.Instance.GetOrCreateController<T>();
    }
}
