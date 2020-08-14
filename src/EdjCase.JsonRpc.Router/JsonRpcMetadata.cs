using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdjCase.JsonRpc.Router.Defaults;

namespace EdjCase.JsonRpc.Router
{
    public static class Extensions
    {
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            return (T) type.GetCustomAttribute(typeof(T));
        }
    }

    public class RpcAnnotationAttribute : Attribute
    {
        public string Annotation { get; }

        public RpcAnnotationAttribute(string annotation)
        {
            this.Annotation = annotation;
        }
    }

    public interface IJsonRpcMetadata
    {
        IEnumerable<SimpleRouteInfo> GetRpcMethodInfos();
    }

    public class SimpleParameterInfo
    {
        public SimpleParameterInfo(string name, Type type, string? anotation)
        {
            this.Name = name;
            this.Type = type;
            this.Anotation = anotation;
        }
        public Type Type { get; set; }
        public string Name { get; set; }
        public string? Anotation { get; set; }
    }
    
    public class SimpleMethdInfo
    {
        public SimpleMethdInfo(string name, Type returningType, string? annotation, SimpleParameterInfo[] parameters)
        {
            this.Name = name;
            this.Annotation = annotation;
            this.Parameters = parameters;
            this.ReturningType = returningType;
        }
        public string Name { get; set; }
        public Type ReturningType { get; set; }
        public string? Annotation { get; set; }
        public SimpleParameterInfo[] Parameters { get; set; }
    }
    
    public class SimpleRouteInfo
    {
        public SimpleRouteInfo(string? route, string? annotation, SimpleMethdInfo[] methodInfos)
        {
            this.Route = route;
            this.Annotation = annotation;
            this.MethodInfos = methodInfos;
        }
        public string? Route { get; set; }   
        public string? Annotation { get; set; }
        public SimpleMethdInfo[] MethodInfos { get; set; }
    }

    internal class JsonRpcMetadata : IJsonRpcMetadata
    {
        public static StaticRpcMethodData? StaticRpcMethodData { get; internal set; }

        public IEnumerable<SimpleRouteInfo> GetRpcMethodInfos()
        {
            if(JsonRpcMetadata.StaticRpcMethodData == null)
                return Enumerable.Empty<SimpleRouteInfo>();

            var rpcMethods = JsonRpcMetadata.StaticRpcMethodData.Methods
                .Select(x =>
                {
                    return new SimpleRouteInfo(
                        x.Key,
                        x.Value.FirstOrDefault()?.DeclaringType?
                            .GetCustomAttribute<RpcAnnotationAttribute>()?.Annotation,
                        this.GetMethodInfos(x.Value));
                }).ToList();

            rpcMethods.Add(new SimpleRouteInfo(null, null,
                this.GetMethodInfos(JsonRpcMetadata.StaticRpcMethodData.BaseMethods)));
            
            return rpcMethods;
        }

        private SimpleMethdInfo[] GetMethodInfos(List<MethodInfo> methodInfos)
        {
            return methodInfos.Select(x =>
            {
                return new SimpleMethdInfo(
                    x.Name,
                    x.ReturnType,
                    x.GetCustomAttribute<RpcAnnotationAttribute>()?.Annotation,
                    this.GetParametersInfos(x));
            }).ToArray();
        }

        private SimpleParameterInfo[] GetParametersInfos(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Select(p => new SimpleParameterInfo(
                p.Name,
                p.ParameterType,
                p.GetCustomAttribute<RpcAnnotationAttribute>()?.Annotation)).ToArray();
        }
    }
}
