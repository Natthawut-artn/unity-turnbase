using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static Dictionary<Type, object> _services = new();

    public static void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public static void Register(object service)
    {
        var type = service.GetType();
        _services[type] = service;

        var interfaces = type.GetInterfaces();
        foreach (var i in interfaces)
        {
            if (!_services.ContainsKey(i))
                _services[i] = service;
        }
    }

    public static T Get<T>()
    {
        return (T)_services[typeof(T)];
    }
}

