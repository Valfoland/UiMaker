﻿using Basement.OEPFramework.Futures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Utils.Threads
{
    public class SingleThreadExecutor : IExecutor
    {
        public int TaskCount => _taskCount;

        private readonly Queue<IFuture> _tasks = new Queue<IFuture>(128);
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly object _syncRoot = new object();
        private volatile bool _shutdown;
        private readonly Thread _thread;
        private int _taskCount;

        public SingleThreadExecutor(ThreadPriority threadPriority = ThreadPriority.Normal)
        {
            _thread = new Thread(Work) { Priority = threadPriority, IsBackground = true };
            _thread.Start();
            while (!_thread.IsAlive) { }
        }

        public void Shutdown()
        {
            lock (_syncRoot)
            {
                _shutdown = true;
                _manualResetEvent.Set();
            }
        }

        public T Execute<T>(T future) where T : IFuture
        {
            if (_shutdown) throw new Exception("executor was shutdown");

            Interlocked.Increment(ref _taskCount);
            lock (_syncRoot)
            {
                _tasks.Enqueue(future);
                _manualResetEvent.Set();
            }

            return future;
        }

        public IFuture Execute(Action action)
        {
            if (_shutdown) throw new Exception("executor Shutdown");

            Interlocked.Increment(ref _taskCount);
            IFuture future = new FutureTask(action);
            lock (_syncRoot)
            {
                _tasks.Enqueue(future);
                _manualResetEvent.Set();
            }

            return future;
        }

        public IFuture Execute<T>(Func<T> func)
        {
            if (_shutdown) throw new Exception("executor Shutdown");

            Interlocked.Increment(ref _taskCount);
            IFuture future = new FutureTask<T>(func);
            lock (_syncRoot)
            {
                _tasks.Enqueue(future);
                _manualResetEvent.Set();
            }

            return future;
        }

        public void Join()
        {
            _thread.Join();
        }

        private void Work()
        {
            _thread.Name = $"Single thread executor {_thread.ManagedThreadId}";
            while (!_shutdown)
            {
                lock (_syncRoot)
                {
                    if (_tasks.Count == 0 && !_shutdown)
                    {
                        _manualResetEvent.Reset();
                    }
                }

                _manualResetEvent.WaitOne();

                while (_tasks.Count > 0)
                {
                    IFuture future;

                    lock (_syncRoot)
                    {
                        future = _tasks.Dequeue();
                    }

                    future.Run();
                    Interlocked.Decrement(ref _taskCount);
                }
            }
        }
    }
}