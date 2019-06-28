using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeployMe.Http.WebApiExtensions.Utility
{
    public static class Parsing
    {
        private static Dictionary<Type, MethodInfo> ParseMethods { get; } = new Dictionary<Type, MethodInfo>();

        public static MethodInfo GetParseMethod(Type type)
        {
            if (ParseMethods.ContainsKey(type))
            {
                return ParseMethods[type];
            }

            MethodInfo parseMethod = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .FirstOrDefault(
                    i =>
                    {
                        ParameterInfo[] parameters = i.GetParameters();
                        return i.Name == "Parse" &&
                               parameters.Length > 0 &&
                               parameters.First().ParameterType == typeof(string) &&
                               parameters.Skip(1).All(j => j.IsOptional);
                    });

            ParseMethods[type] = parseMethod;
            return parseMethod;
        }
    }
}
