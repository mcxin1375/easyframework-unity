using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace EasyFramework
{
    internal class ControllerManager : Singleton<ControllerManager>, IControllerManager, IFrameworkTick
    {
        private enum Operation { None, Creating, Entering, Exiting, Destroying }

        private sealed class Entry
        {
            public readonly Type Type;
            public IController Controller;
            public Operation Operation;
            public bool InStack;
            public long Generation;
            public long ActivationVersion;

            public Entry(Type type) => Type = type;
        }

        private readonly struct Registration
        {
            public readonly Entry Entry;
            public readonly long Generation;
            public Registration(Entry entry) { Entry = entry; Generation = entry.Generation; }
        }

        private readonly Dictionary<Type, Entry> _entries = new();
        private readonly List<IController> _enterList = new();
        private readonly List<Registration> _updateList = new();
        private Entry _active;
        private bool _refresh;

        public IController TopController => _enterList.Count > 0 ? _enterList[^1] : null;
        public IReadOnlyList<IController> EnterList => _enterList;
        public bool HasEnter(IController controller) => TryGetEntry(controller, out var entry) && entry.InStack;
        public bool HasActive(IController controller) => TryGetEntry(controller, out var entry) && ReferenceEquals(_active, entry);
        public T GetController<T>() where T : class, IController
            => _entries.TryGetValue(typeof(T), out var entry) ? entry.Controller as T : null;

        protected override void OnCreate() => FBehaviour.Instance.Add(this);
        protected override void OnDestroy() => FBehaviour.Instance?.Remove(this);

        internal T GetOrCreateController<T>() where T : class, IController, new()
            => (T)GetOrCreateEntry<T>().Controller;

        private Entry GetOrCreateEntry<T>() where T : class, IController, new()
        {
            var type = typeof(T);
            if (_entries.TryGetValue(type, out var existing))
            {
                if (existing.Controller == null) throw new InvalidOperationException($"Controller {type.Name} is being constructed.");
                return existing;
            }

            var entry = new Entry(type) { Operation = Operation.Creating };
            _entries.Add(type, entry);
            try
            {
                entry.Controller = new T();
                entry.Controller.OnCreate();
                return entry;
            }
            catch
            {
                _entries.Remove(type);
                throw;
            }
            finally { entry.Operation = Operation.None; }
        }

        private static void BeginOperation(Entry entry, Operation operation)
        {
            if (entry.Operation != Operation.None)
                throw new InvalidOperationException($"Controller {entry.Type.Name} is already {entry.Operation}.");
            entry.Operation = operation;
        }

        private Entry BeginEnter<T>() where T : class, IController, new()
        {
            var entry = GetOrCreateEntry<T>();
            BeginOperation(entry, Operation.Entering);
            return entry;
        }

        private Entry BeginEnter(IController controller)
        {
            if (!TryGetEntry(controller, out var entry))
                throw new InvalidOperationException("The Controller instance is not owned by this manager.");
            BeginOperation(entry, Operation.Entering);
            return entry;
        }

        public ETask<T> EnterAsync<T>() where T : class, IController, new()
        {
            var entry = BeginEnter<T>();
            try { if (entry.Controller is ITParams parameters) parameters.SetParamsDefault(); }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        internal ETask<T> EnterInstanceAsync<T>(T controller) where T : class, IController
        {
            var entry = BeginEnter(controller);
            try { if (controller is ITParams parameters) parameters.SetParamsDefault(); }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        public ETask<T> EnterAsync<T, T1>(in T1 p1) where T : class, IController, ITParams<T1>, new()
        {
            var entry = BeginEnter<T>();
            try { ((T)entry.Controller).SetParams(in p1); }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        public ETask<T> EnterAsync<T, T1, T2>(in T1 p1, in T2 p2)
            where T : class, IController, ITParams<T1, T2>, new()
        {
            var entry = BeginEnter<T>();
            try { ((T)entry.Controller).SetParams(in p1, in p2); }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        public ETask<T> EnterAsync<T, T1, T2, T3>(in T1 p1, in T2 p2, in T3 p3)
            where T : class, IController, ITParams<T1, T2, T3>, new()
        {
            var entry = BeginEnter<T>();
            try { ((T)entry.Controller).SetParams(in p1, in p2, in p3); }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        internal ETask<T> EnterWithParamsAsync<T, T1>(T controller, in T1 p1)
            where T : class, IController
        {
            var entry = BeginEnter(controller);
            try
            {
                if (controller is not ITParams<T1> parameters)
                    throw new ArgumentException($"Controller {entry.Type.Name} does not implement the supplied ITParams signature.");
                parameters.SetParams(in p1);
            }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        internal ETask<T> EnterWithParamsAsync<T, T1, T2>(T controller, in T1 p1, in T2 p2)
            where T : class, IController
        {
            var entry = BeginEnter(controller);
            try
            {
                if (controller is not ITParams<T1, T2> parameters)
                    throw new ArgumentException($"Controller {entry.Type.Name} does not implement the supplied ITParams signature.");
                parameters.SetParams(in p1, in p2);
            }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        internal ETask<T> EnterWithParamsAsync<T, T1, T2, T3>(T controller, in T1 p1, in T2 p2, in T3 p3)
            where T : class, IController
        {
            var entry = BeginEnter(controller);
            try
            {
                if (controller is not ITParams<T1, T2, T3> parameters)
                    throw new ArgumentException($"Controller {entry.Type.Name} does not implement the supplied ITParams signature.");
                parameters.SetParams(in p1, in p2, in p3);
            }
            catch { entry.Operation = Operation.None; throw; }
            return EnterCore<T>(entry);
        }

        private async ETask<T> EnterCore<T>(Entry entry) where T : class, IController
        {
            try
            {
                await entry.Controller.OnPreEnterAsync();

                entry.InStack = true;
                entry.Generation++;
                _enterList.Remove(entry.Controller);
                _enterList.Add(entry.Controller);
                _refresh = true;
                RefreshActive();

                await entry.Controller.OnEnterAsync();
            }
            finally { entry.Operation = Operation.None; }

            return (T)entry.Controller;
        }

        public ETask ExitAsync<T>() where T : class, IController
            => _entries.TryGetValue(typeof(T), out var entry) ? ExitCore(entry) : ETask.CompletedTask;

        public ETask ExitAsync(IController controller)
            => TryGetEntry(controller, out var entry) ? ExitCore(entry) : ETask.CompletedTask;

        private async ETask ExitCore(Entry entry)
        {
            BeginOperation(entry, Operation.Exiting);
            try
            {
                if (!entry.InStack) return;
                await ExitStages(entry);
            }
            finally { entry.Operation = Operation.None; }

        }

        private async ETask ExitStages(Entry entry)
        {
            entry.InStack = false;
            _enterList.Remove(entry.Controller);
            _refresh = true;
            RefreshActive();

            await entry.Controller.OnExitAsync();
        }

        public async ETask ExitAllAsync()
        {
            var batch = CaptureEntered();
            List<Exception> errors = null;
            for (int i = batch.Length - 1; i >= 0; i--)
            {
                if (!IsCurrent(batch[i])) continue;
                try { await ExitCore(batch[i].Entry); }
                catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            }
            if (errors?.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
            if (errors?.Count > 1) throw new AggregateException(errors);
        }

        internal ETask DestroyAsync<T>() where T : class, IController
            => _entries.TryGetValue(typeof(T), out var entry) ? DestroyCore(entry) : ETask.CompletedTask;

        internal ETask DestroyAsync(IController controller)
            => TryGetEntry(controller, out var entry) ? DestroyCore(entry) : ETask.CompletedTask;

        private async ETask DestroyCore(Entry entry)
        {
            BeginOperation(entry, Operation.Destroying);
            try
            {
                if (entry.InStack)
                {
                    await ExitStages(entry);
                }
                entry.Controller.OnDestroy();
                _entries.Remove(entry.Type);
                _refresh = true;
            }
            finally { entry.Operation = Operation.None; }

        }

        private bool TryGetEntry(IController controller, out Entry entry)
        {
            if (controller != null && _entries.TryGetValue(controller.GetType(), out entry)
                && ReferenceEquals(entry.Controller, controller)) return true;
            entry = null;
            return false;
        }

        private void RefreshActive()
        {
            TryGetEntry(TopController, out var next);
            if (ReferenceEquals(next, _active)) return;
            var previous = _active;
            _active = next;
            if (previous != null) previous.ActivationVersion++;
            long nextVersion = next == null ? 0 : ++next.ActivationVersion;
            previous?.Controller.OnSetActive(false);
            if (next != null && next.ActivationVersion == nextVersion && ReferenceEquals(_active, next))
                next.Controller.OnSetActive(true);
        }

        private Registration[] CaptureEntered()
        {
            var batch = new Registration[_enterList.Count];
            for (int i = 0; i < batch.Length; i++) batch[i] = new Registration(_entries[_enterList[i].GetType()]);
            return batch;
        }

        private bool IsCurrent(Registration registration)
            => registration.Entry.InStack && registration.Entry.Generation == registration.Generation
                && TryGetEntry(registration.Entry.Controller, out var current) && ReferenceEquals(current, registration.Entry);

        void IFrameworkTick.OnUpdate()
        {
            if (_refresh)
            {
                _refresh = false;
                _updateList.Clear();
                foreach (var controller in _enterList)
                    _updateList.Add(new Registration(_entries[controller.GetType()]));
            }
            DispatchFrame(false);
        }

        void IFrameworkTick.OnLateUpdate() => DispatchFrame(true);
        void IFrameworkTick.OnDestroy() => OnDestroy();

        private void DispatchFrame(bool late)
        {
            foreach (var registration in _updateList)
            {
                try
                {
                    if (!IsCurrent(registration) || !registration.Entry.Controller.UpdateEnabled) continue;
                    var entry = registration.Entry;
                    if (late) entry.Controller.OnLateUpdate();
                    else entry.Controller.OnUpdate();
                }
                catch (Exception error) { FDebug.LogException(error); }
            }
        }
    }
}
