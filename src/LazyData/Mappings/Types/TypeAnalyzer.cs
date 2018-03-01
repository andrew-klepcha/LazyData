﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LazyData.Attributes;
using LazyData.Extensions;
using LazyData.Mappings.Types.Primitives;
using LazyData.Mappings.Types.Primitives.Checkers;

namespace LazyData.Mappings.Types
{
    public class TypeAnalyzer : ITypeAnalyzer
    {
        public TypeAnalyzerConfiguration Configuration { get; }
        public IPrimitiveHandler PrimitiveHandler { get; }

        public TypeAnalyzer(IPrimitiveHandler primitiveHandler = null, TypeAnalyzerConfiguration configuration = null)
        {
            PrimitiveHandler = primitiveHandler ?? new PrimitiveHandler();
            PrimitiveHandler.AddPrimitiveCheck(new BasicPrimitiveChecker());
            Configuration = configuration ?? new TypeAnalyzerConfiguration();
        }

        public bool IsGenericList(Type type)
        { return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>); }

        public bool IsGenericDictionary(Type type)
        { return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>); }

        public bool IsDynamicType(Type type)
        { return type.IsAbstract || type.IsInterface || type == typeof(object); }

        public bool IsDynamicType(PropertyInfo propertyInfo)
        {
            var typeIsDynamic = IsDynamicType(propertyInfo.PropertyType);
            return typeIsDynamic || propertyInfo.HasAttribute<DynamicTypeAttribute>();
        }

        public bool HasIgnoredTypes()
        { return Configuration.IgnoredTypes.Any(); }

        public bool IsIgnoredType(Type type)
        { return !Configuration.IgnoredTypes.Any(type.IsAssignableFrom); }

        public bool IsTypeMatch(Type actualType, Type expectedType)
        {
            if (actualType == expectedType || actualType.IsAssignableFrom(expectedType))
            { return true; }

            if (actualType.IsGenericType)
            {
                var genericType = actualType.GetGenericTypeDefinition();
                if (genericType == expectedType)
                { return true; }

                var genericInterfaces = genericType.GetInterfaces();
                if (genericInterfaces.Any(x => x.Name == expectedType.Name))
                { return true; }
            }

            var interfaces = actualType.GetInterfaces();
            if (interfaces.Any(x => x == expectedType))
            { return true; }
            
            return false;
        }

        public virtual bool IsDefaultPrimitiveType(Type type)
        {
            return PrimitiveHandler.IsKnownPrimitive(type);
        }

        public Type GetNullableType(Type possibleNullable)
        { return Nullable.GetUnderlyingType(possibleNullable); }

        public bool ShouldTreatAsPrimitiveType(Type type)
        {
            return PrimitiveHandler.IsKnownPrimitive(type);
        }

        public bool IsPrimitiveType(Type type)
        {
            var isDefaultPrimitive = IsDefaultPrimitiveType(type);

            if (isDefaultPrimitive)
            { return true; }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                var isNullablePrimitive = IsDefaultPrimitiveType(nullableType);
                if (isNullablePrimitive) { return true; }
            }

            return ShouldTreatAsPrimitiveType(type);
        }
    }
}