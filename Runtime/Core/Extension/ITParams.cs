
namespace EasyFramework
{
    public interface ITParams
    {
        void SetParamsDefault() { }
        void SetParams(object[] tObjects);
    }
    public interface ITParams<TValue1> : ITParams
    {
        ref readonly TValue1 T1 { get; }
        void SetParams(in TValue1 t1);
    }
    public interface ITParams<TValue1, TValue2> : ITParams
    {
        ref readonly TValue1 T1 { get; }
        ref readonly TValue2 T2 { get; }
        void SetParams(in TValue1 t1, in TValue2 t2);
    }
    public interface ITParams<TValue1, TValue2, TValue3> : ITParams
    {
        ref readonly TValue1 T1 { get; }
        ref readonly TValue2 T2 { get; }
        ref readonly TValue3 T3 { get; }
        void SetParams(in TValue1 t1, in TValue2 t2, in TValue3 t3);
    }
}