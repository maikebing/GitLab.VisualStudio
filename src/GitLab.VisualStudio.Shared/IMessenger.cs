using System;

namespace GitLab.VisualStudio.Shared
{
    public interface IMessenger
    {
        void Send(string command);
        void Register(string command, Action action);

        void Send<T>(string command, T t);
        void Register<T>(string command, Action<T> action);

        void Send<T1, T2>(string command, T1 t1, T2 t2);
        void Register<T1, T2>(string command, Action<T1, T2> action);

        void Send<TResult>(string command, Action<TResult> callback);
        void Register<TReturn>(string command, Func<TReturn> action);

        void Send<T, TResult>(string command, T message, Action<TResult> callback);
        void Register<T, TReturn>(string command, Func<T, TReturn> action);

        void Send<T1, T2, TResult>(string command, T1 t1, T2 t2, Action<TResult> callback);
        void Register<T1, T2, TReturn>(string command, Func<T1, T2, TReturn> action);

        void UnRegister(string command, Delegate action);
        void UnRegister(object target);
    }
}
