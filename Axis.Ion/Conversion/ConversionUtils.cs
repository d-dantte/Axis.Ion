using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.Conversion
{
    public static class ConversionUtils
    {
        internal static readonly string ObjectRefPrefix = "@Ref-";

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static IEnumerable<TItem> ThrowIfContainsNull<TItem>(this IEnumerable<TItem> enumerable, Exception exception)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            return enumerable.Select(item =>
            {
                if (item is null)
                    exception.Throw();

                return item;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static TypeCategory CategoryOf(Type clrType)
        {
            if (clrType is null)
                throw new ArgumentNullException(nameof(clrType));

            var type = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            if (IsPrimitive(type))
                return TypeCategory.Primitive;

            if (type.IsEnum)
                return TypeCategory.Enum;

            if (IsSingleDimensionArray(type))
                return TypeCategory.SingleDimensionArray;

            if (IsMap(type))
                return HasNonDefaultWritableProperties(type, "Item")
                    ? TypeCategory.ComplexMap
                    : TypeCategory.Map;

            if (IsCollection(type))
                return  HasNonDefaultWritableProperties(type, "Item", "Capacity")
                    ? TypeCategory.ComplexCollection
                    : TypeCategory.Collection;

            if (IsObject(type))
                return TypeCategory.Object;

            return TypeCategory.InvalidType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ionType"></param>
        /// <param name="clrType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal static bool AreCompatible(IonTypes ionType, Type clrType)
        {
            if (clrType is null)
                throw new ArgumentNullException(nameof(clrType));

            var targetType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return ionType switch
            {
                IonTypes.Bool => typeof(bool).Equals(clrType) || typeof(bool?).Equals(clrType),
                IonTypes.Int => targetType.IsIntegral(out _),
                IonTypes.Float => targetType.IsReal(out _),
                IonTypes.Decimal => targetType.IsDecimal(out _),
                IonTypes.Timestamp => targetType.IsDateTime(out _),
                IonTypes.String => typeof(string).Equals(clrType),
                IonTypes.OperatorSymbol => typeof(string).Equals(clrType),
                IonTypes.TextSymbol => typeof(string).Equals(clrType),
                IonTypes.Blob => targetType.Implements(typeof(ICollection<byte>)),
                IonTypes.Clob => targetType.Implements(typeof(ICollection<byte>)),
                IonTypes.List => targetType.ImplementsGenericInterface(typeof(ICollection<>)),
                IonTypes.Sexp => targetType.Implements(typeof(ICollection<string>)),
                IonTypes.Struct => CategoryOf(targetType) == TypeCategory.Object,
                _ => throw new ArgumentException($"Invalid ion type: {ionType}")
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ionType"></param>
        /// <param name="containerItemType">For cases of arrays, maps and lists, this type is non-null and represents the item-type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static Type CompatibleClrType(IonTypes ionType, Type? containerItemType = null)
        {
            var ionToClrType = ionType switch
            {
                IonTypes.Bool => typeof(bool),
                IonTypes.Int => typeof(int),
                IonTypes.Float => typeof(double),
                IonTypes.Decimal => typeof(decimal),
                IonTypes.Timestamp => typeof(DateTimeOffset),
                IonTypes.String => typeof(string),
                IonTypes.TextSymbol => typeof(string),
                IonTypes.OperatorSymbol => typeof(string),
                IonTypes.Blob => typeof(byte[]),
                IonTypes.Clob => typeof(byte[]),
                IonTypes.Sexp => typeof(List<>).MakeGenericType(containerItemType ?? throw new ArgumentNullException(nameof(containerItemType))),
                IonTypes.List => typeof(List<>).MakeGenericType(containerItemType ?? throw new ArgumentNullException(nameof(containerItemType))),
                IonTypes.Struct => containerItemType ?? typeof(object),
                IonTypes.Null => typeof(object),
                _ => throw new ArgumentException($"Invalid ion-type: {ionType}")
            };

            return ionToClrType;
        }

        internal static IonTypes CompatibleIonType(Type clrType)
        {
            return CategoryOf(clrType) switch
            {
                TypeCategory.Object => IonTypes.Struct,
                TypeCategory.ComplexMap => IonTypes.Struct,
                TypeCategory.ComplexCollection => IonTypes.Struct,
                TypeCategory.Map => IonTypes.Struct,
                TypeCategory.Collection => IonTypes.List,
                TypeCategory.SingleDimensionArray => IonTypes.List,
                TypeCategory.Enum => IonTypes.TextSymbol,
                TypeCategory.Primitive =>
                    clrType.IsIntegral(out _) ? IonTypes.Int:
                    clrType.IsReal(out _) ? IonTypes.Float:
                    clrType.IsDecimal(out _) ? IonTypes.Decimal:
                    clrType.IsBoolean(out _) ? IonTypes.Bool:
                    clrType.IsDateTime(out _) ? IonTypes.Timestamp:
                    clrType.IsString() ? IonTypes.String:
                    throw new ArgumentException($"Invalid primitive clr type: {clrType}"),
                _ => throw new ArgumentException($"Invalid clr type: {clrType}")
            };
        }

        /// <summary>
        /// Checks that the <paramref name="targetType"/> is assignable from <paramref name="instanceType"/>
        /// </summary>
        /// <param name="targetType">The source type</param>
        /// <param name="instanceType">the instance type</param>
        internal static void ValidateCongruenceWith(this Type targetType, Type? instanceType)
        {
            if (!targetType.IsCongruentWith(instanceType))
                throw new ArgumentException($"source-type: {targetType}, and instance-type: {instanceType?.ToString() ?? ("null")}, are incompatible");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static bool IsCongruentWith(this Type targetType, Type? instanceType)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            return instanceType is null
                || targetType.IsAssignableFrom(instanceType);
        }


        /// <summary>
        /// Specialized method that converts the ion to an object, resolving object-refs if found
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="ion"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static object? ToClrObject(this
            IIonValue ion,
            Type targetType,
            ConversionContext context)
        {
            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            var objectIdAnnotation = ion.Annotations.FirstOrNull(ann => ann.Value.StartsWith(ObjectRefPrefix));

            var objectId = objectIdAnnotation?.Value.TrimStart(ObjectRefPrefix);

            return objectId switch
            {
                null => Ionizer.ToClr(targetType, ion, context),
                _ => context.GetOrAdd(objectId, _ => Ionizer.ToClr(targetType, ion, context))
            };
        }


        internal static IIonValue ToIonValue(this
            object? clrValue,
            Type targetType,
            ConversionContext context)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            var clrType = clrValue?.GetType() ?? targetType;

            if (clrValue is null)
                return IIonValue.NullOf(CompatibleIonType(clrType));

            var ion = CategoryOf(targetType) switch
            {
                TypeCategory.Enum => Ionizer.ToIon(clrType, clrValue, context),
                TypeCategory.Primitive => Ionizer.ToIon(clrType, clrValue, context),
                TypeCategory.InvalidType => throw new ArgumentException($"Invalid target type: {targetType}"),
                _ => context.TryTrack(clrValue, out var id)
                    ? Ionizer.ToIon(clrType, clrValue, context)
                    : new IonTextSymbol($"{ObjectRefPrefix}{id}")
            };
            return ion;
        }

        private static bool IsPrimitive(Type type)
        {
            if (type.IsPrimitive)
                return true;

            if (typeof(DateTimeOffset).Equals(type))
                return true;

            if (typeof(DateTime).Equals(type))
                return true;

            if (typeof(string).Equals(type))
                return true;

            if (typeof(decimal).Equals(type))
                return true;

            return false;
        }

        /// <summary>
        /// A map is any type that implements <see cref="IDictionary{TKey, TValue}"/>, where the key is a <see cref="string"/>
        /// </summary>
        /// <param name="type">the type to check</param>
        /// <returns>true if it is a map, false otherwise</returns>
        private static bool IsMap(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (IsBaseDictionaryInterface(type))
                return typeof(string).Equals(type.GetGenericArguments()[0]);

            else return type
                .ImplementsGenericInterface(typeof(IDictionary<,>))
                && typeof(string).Equals(
                    type.GetGenericInterface(typeof(IDictionary<,>))
                        .GetGenericArguments()[0]);
        }

        /// <summary>
        /// A collection is any non-array type that is, or implements <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="type">the type to check</param>
        /// <returns>true if it is a list, false otherwise</returns>
        private static bool IsCollection(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return
                !type.IsArray
                && (type.ImplementsGenericInterface(typeof(IEnumerable<>))
                || type.HasGenericInterfaceDefinition(typeof(IEnumerable<>)));
        }

        /// <summary>
        /// Checks that the type has writable properties, excluding properties found in the <paramref name="excludeProperties"/> set.
        /// </summary>
        /// <param name="type">the type to check</param>
        /// <param name="excludeProperties">properties to exclude</param>
        /// <returns>true if writable properties are present, false otherwise</returns>
        private static bool HasNonDefaultWritableProperties(Type type, params string[] excludeProperties)
        {
            var exclusion = new HashSet<string>(excludeProperties);
            return type
                .GetProperties()
                .Where(prop => !exclusion.Contains(prop.Name))
                .Any(prop => prop.CanWrite);
        }

        /// <summary>
        /// Indicates if the type represents a single dimension array
        /// </summary>
        /// <param name="type">the type to check</param>
        /// <returns>true if it is a single dimension array, false otherwise</returns>
        private static bool IsSingleDimensionArray(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsSZArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static bool IsObject(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.Extends(typeof(Delegate)))
                return false;

            if (type.IsArray)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsBaseDictionaryInterface(Type type)
        {
            return type.IsGenericType
                && typeof(IDictionary<,>).Equals(type.GetGenericTypeDefinition());
        }
    }
}
