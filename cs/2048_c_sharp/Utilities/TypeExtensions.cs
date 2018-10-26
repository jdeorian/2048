using System;
using System.Reflection;

using LinqToDB.Mapping;

namespace _2048_c_sharp.Utilities
{
    public static class TypeExtensions
    {
        public static string GetTableName(this Type type) => type.GetCustomAttribute<TableAttribute>()?.Name;
    }
}
