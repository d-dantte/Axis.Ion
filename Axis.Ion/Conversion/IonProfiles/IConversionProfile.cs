using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.Conversion.IonProfiles
{
    public interface IConversionProfile : IConverter
    {
        #region Static Helpers - Move all of these into a separate helper util class

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
                return HasNonDefaultWritableProperties(type, "Item", "Capacity")
                    ? TypeCategory.ComplexCollection
                    : TypeCategory.Collection;

            if (IsObject(type))
                return TypeCategory.Object;

            return TypeCategory.InvalidType;
        }

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
                IonTypes.IdentifierSymbol => typeof(string).Equals(clrType),
                IonTypes.QuotedSymbol => typeof(string).Equals(clrType),
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
                IonTypes.IdentifierSymbol => typeof(string),
                IonTypes.QuotedSymbol => typeof(string),
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

        /// <summary>
        /// Checks that the <paramref name="sourceType"/> is assignable from <paramref name="instanceType"/>
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="instanceType">the instance type</param>
        internal static void ValidateSourceTypeCompatibility(Type sourceType, Type? instanceType)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            if (instanceType is not null
                && !sourceType.IsAssignableFrom(instanceType))
                throw new ArgumentException($"source-type: {sourceType}, and instance-type: {instanceType}, are incompatible");
        }

        private static bool IsPrimitive(Type type)
        {
            if (type.IsPrimitive)
                return true;

            if (typeof(DateTimeOffset).Equals(type))
                return true;

            if (typeof(DateTime).Equals(type))
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

        private static bool IsBaseDictionaryInterface(Type type)
        {
            return type.IsGenericType
                && typeof(IDictionary<,>).Equals(type.GetGenericTypeDefinition());
        }

        #endregion
    }
}
