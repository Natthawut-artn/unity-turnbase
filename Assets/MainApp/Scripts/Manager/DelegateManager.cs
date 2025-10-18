namespace DelegateManager
{
    public class Delegate
    {
        public delegate void CallBack();
        public delegate void CallBackWithInt(int value);
        public delegate void CallBackWith2Int(int value1, int value2);
    }
}