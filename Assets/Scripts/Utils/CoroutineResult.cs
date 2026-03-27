namespace SelStrom.Asteroids
{
    public class CoroutineResult
    {
        public bool IsSuccess { get; protected set; }
        public string Error { get; protected set; }

        public void SetSuccess() => IsSuccess = true;

        public void SetError(string error)
        {
            IsSuccess = false;
            Error = error;
        }
    }

    public class CoroutineResult<T> : CoroutineResult
    {
        public T Value { get; private set; }

        public void SetSuccess(T value)
        {
            IsSuccess = true;
            Value = value;
        }
    }
}
