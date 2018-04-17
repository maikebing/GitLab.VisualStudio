using GitLab.VisualStudio.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IMessenger))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Messenger : IMessenger
    {
        #region Miscellaneous

        private readonly IDictionary<string, HashSet<WeakAction>> Registry = new ConcurrentDictionary<string, HashSet<WeakAction>>();

        private class WeakAction
        {
            private readonly MethodInfo _method;

            private readonly WeakReference _reference;

            public WeakAction(Delegate action)
            {
                if (action.Target == null)
                {
                    _reference = new WeakReference(action);
                }
                else
                {
                    _reference = new WeakReference(action.Target);
                }

                _method = action.Method;
            }

            #region Properties & Indexers

            public bool IsAlive
            {
                get { return _reference.IsAlive; }
            }

            public WeakReference Reference
            {
                get { return _reference; }
            }

            #endregion Properties & Indexers

            public bool Match(Delegate action)
            {
                if (action == null)
                {
                    return false;
                }

                var target = action.Target == null ? action : action.Target;

                return target == _reference.Target && _method == action.Method;
            }

            public object Invoke(params object[] args)
            {
                if (_reference.IsAlive)
                {
                    var target = _reference.Target;
                    if (target != null)
                    {
                        return _method.Invoke(target, args);
                    }
                }

                if (_method.ReturnType.IsValueType)
                {
                    return Activator.CreateInstance(_method.ReturnType);
                }

                return null;
            }
        }

        private IList<WeakAction> GetAction(string command)
        {
            // Locking the command registry to avoid multithreaded issue
            lock (Registry)
            {
                HashSet<WeakAction> actions;
                if (Registry.TryGetValue(command, out actions))
                {
                    var lives = actions.Where(o => o.IsAlive).ToList();
                    if (lives.Count > 0)
                    {
                        return lives;
                    }
                    Registry.Remove(command);
                }
            }

            return null;
        }

        private void Invoke(string command, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(command);
            }

            var actions = GetAction(command);

            if (actions != null)
            {
                foreach (var action in actions)
                {
                    action.Invoke(args);
                }
            }
        }

        private void InvokeWithCallback<TResult>(string command, Action<TResult> callback, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(command);
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            var actions = GetAction(command);

            var action = actions.FirstOrDefault();
            if (action != null)
            {
                var result = action.Invoke(args);
                if (result is TResult)
                {
                    callback.Invoke((TResult)result);
                }
            }
        }

        private void RegisterInternal(string command, Delegate action)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(command);
            }

            lock (Registry)
            {
                HashSet<WeakAction> actions;
                if (Registry.TryGetValue(command, out actions))
                {
                    actions.Add(new WeakAction(action));
                }
                else
                {
                    actions = new HashSet<WeakAction>();
                    actions.Add(new WeakAction(action));

                    Registry[command] = actions;
                }
            }
        }

        #endregion Miscellaneous

        public void Send(string command)
        {
            Invoke(command);
        }

        public void Register(string command, Action action)
        {
            RegisterInternal(command, action);
        }

        public void Send<T>(string command, T message)
        {
            Invoke(command, message);
        }

        public void Register<T>(string command, Action<T> action)
        {
            RegisterInternal(command, action);
        }

        public void Send<T1, T2>(string command, T1 t1, T2 t2)
        {
            Invoke(command, t1, t2);
        }

        public void Register<T1, T2>(string command, Action<T1, T2> action)
        {
            RegisterInternal(command, action);
        }

        public void Send<TResult>(string command, Action<TResult> callback)
        {
            InvokeWithCallback(command, callback);
        }

        public void Register<TReturn>(string command, Func<TReturn> action)
        {
            RegisterInternal(command, action);
        }

        public void Send<T, TResult>(string command, T message, Action<TResult> callback)
        {
            InvokeWithCallback(command, callback, message);
        }

        public void Register<T, TReturn>(string command, Func<T, TReturn> action)
        {
            RegisterInternal(command, action);
        }

        public void Send<T1, T2, TResult>(string command, T1 t1, T2 t2, Action<TResult> callback)
        {
            InvokeWithCallback(command, callback, t1, t2);
        }

        public void Register<T1, T2, TReturn>(string command, Func<T1, T2, TReturn> action)
        {
            RegisterInternal(command, action);
        }

        public void UnRegister(string command, Delegate action)
        {
            lock (Registry)
            {
                HashSet<WeakAction> actions;
                if (Registry.TryGetValue(command, out actions))
                {
                    var match = actions.FirstOrDefault(o => o.IsAlive && o.Match(action));
                    if (match != null)
                    {
                        actions.Remove(match);

                        if (actions.Count(o => o.IsAlive) == 0)
                        {
                            Registry.Remove(command);
                        }
                    }
                }
            }
        }

        public void UnRegister(object target)
        {
            lock (Registry)
            {
                foreach (var key in Registry.Keys)
                {
                    var actions = Registry[key];

                    foreach (var action in actions.ToList())
                    {
                        if (action.Reference.Target == target)
                        {
                            actions.Remove(action);
                        }
                    }

                    if (actions.Count == 0)
                    {
                        Registry.Remove(key);
                    }
                }
            }
        }
    }
}