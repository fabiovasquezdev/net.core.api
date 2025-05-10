
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Common
{
  public static class SessionObservableExtension
    {
        public static IServiceCollection AddSessionObservable(this IServiceCollection services) =>
            services.AddSingleton(typeof(ISessionObservable<>), 
                                  typeof(SessionObservable<>));
    }

    public interface ISessionObservable<T>
    {
        public delegate Task Subscriber(T e);
        IDisposable Subscribe(Subscriber subscriber, params object[] keys);
        int SendNotification(T e, params object[] keys);
    }
    
    internal sealed class SessionObservable<T> : ISessionObservable<T>
    {
        private readonly SubscriberConnectionManager _subscribers = new();
        
        private Unsubscriber Subscribe(object key, ISessionObservable<T>.Subscriber subscriber)
        {
            var hash = GenHash(key);
            lock(_subscribers)
                if (!_subscribers.TryAdd(hash, subscriber))
                    _subscribers[hash] += subscriber;
            return new Unsubscriber(hash, _subscribers, subscriber);
        }

        public IDisposable Subscribe(ISessionObservable<T>.Subscriber subscriber, params object[] keys) =>
            keys.Length == 0 ? 
                Subscribe(DateTime.Now.ToString("mm:ss"), subscriber) : 
                new UnsubscriberList(from k in keys select Subscribe(k, subscriber));
        

        private int SendNotification(object key, T e) =>
            SendHashNotification(GenHash(key), e);

        public int SendNotification(T e, params object[] keys) =>
            keys.Length > 0 ?  
                keys.Sum(k => SendNotification(k, e)) :
                _subscribers.Keys.Sum(h=> SendHashNotification(h, e));

        private int SendHashNotification(string hash, T e)
        {
            lock(_subscribers)
            {
                if (_subscribers.TryGetValue(hash, out var subscriber))
                    subscriber(e);
                return _subscribers.GetConectionsByHash(hash);
            }
        }

        private static string GenHash(object key) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(key)));

        private sealed class UnsubscriberList : IDisposable
        {
            public UnsubscriberList() { }
            public UnsubscriberList(IEnumerable<Unsubscriber> unsubscribers) =>
                _unsubscribers = unsubscribers.ToList();
            
            private readonly IList<Unsubscriber> _unsubscribers = [];
            private void Add(Unsubscriber subscriber) => 
                _unsubscribers.Add(subscriber);
            
            public void Dispose()
            {
                foreach (var u in _unsubscribers)
                    u.Dispose();
            }

            public static UnsubscriberList operator + (UnsubscriberList u1, 
                                                       Unsubscriber u2)
            {
                u1.Add(u2);
                return u1;
            }
        }

        private sealed class Unsubscriber(string hash, 
                                          SubscriberConnectionManager subscriberConnectionManager, 
                                          ISessionObservable<T>.Subscriber subscriber) : IDisposable
        {
            private readonly SubscriberConnectionManager _subscriberConnectionManager = subscriberConnectionManager;
            private readonly string _hash = hash;
            private readonly ISessionObservable<T>.Subscriber _subscriber = subscriber;
            public void Dispose() =>
                _subscriberConnectionManager.Disconnect(_hash, _subscriber);

            public static UnsubscriberList operator + (Unsubscriber u1, 
                                                       Unsubscriber u2) => new([u1, u2]);
        }

        private sealed class ConnectionManager : Dictionary<string, int>
        {
            private readonly Dictionary<string, DateTime> _createdAtConnections = [];

            public static int operator + (ConnectionManager c, string key) 
            {
                lock (c)
                    return c.TryAdd(key) ? c[key] : ++c[key];
            }

            public static int operator - (ConnectionManager c, string key) 
            {
                lock (c)
                {
                    var cc = --c[key];
                    if (cc != 0)
                        return cc;
                        
                    c.Remove(key);
                    c._createdAtConnections.Remove(key);
                    return cc;
                }
            }

            private bool TryAdd(string key)
            {
                var r = TryAdd(key, 1);
                if (r)
                    _createdAtConnections.Add(key, DateTime.Now);
                return r;
            }   
        }

        private sealed class SubscriberConnectionManager : Dictionary<string, ISessionObservable<T>.Subscriber>
        {
            public int AlliveGlobalConnections;
            private readonly ConnectionManager _alliveConnections = [];

            public new bool TryAdd(string key, ISessionObservable<T>.Subscriber value)
            {
                Interlocked.Increment(ref AlliveGlobalConnections);
                var k = _alliveConnections + key;

                Console.WriteLine("[NEW CONNECTION {0}] - Global Allive connections: {1}, Key Connections: {2}", 
                key, AlliveGlobalConnections, k);

                return base.TryAdd(key, value);
            }

            public void Disconnect(string hash, ISessionObservable<T>.Subscriber subscriber)
            {
                Interlocked.Decrement(ref AlliveGlobalConnections);
                lock(this)
                {
                    var ac = _alliveConnections - hash;
                    this[hash] -= subscriber;
                    if (ac == 0)
                        Remove(hash);

                    Console.WriteLine("[CLOSE CONNECTION {0}] - Global Allive connections: {1}, Key Connections: {2}", 
                    hash, AlliveGlobalConnections, ac);
                }
            }

            public int GetConectionsByHash(string hash)
            {
                lock (_alliveConnections)
                    return _alliveConnections.TryGetValue(hash, out var value) ? value : 0;
            }
        }
    }
}