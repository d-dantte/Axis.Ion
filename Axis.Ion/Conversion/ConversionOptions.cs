using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Axis.Luna.Extensions;

namespace Axis.Ion.Conversion
{
    #region Enums
    /// <summary>
    /// Determines how null values are processed when converting Clr pocos to ion
    /// </summary>
    public enum NullValueBehavior
    {
        Include,
        Ignore
    }

    /// <summary>
    /// Determines how default struct-values are processed when converting Clr pocos to ion
    /// </summary>
    public enum DefaultValueBehavior
    {
        Include,
        Ignore
    }
    #endregion

    public struct ConversionOptions
    {
        /// <summary>
        /// A readonly list of <see cref="IClrConverter"/> instances
        /// </summary>
        public IImmutableList<IClrConverter> ClrConverters { get; }

        /// <summary>
        /// A readonly list of <see cref="IIonConverter"/> instances
        /// </summary>
        public IImmutableList<IIonConverter> IonConverters { get; }

        /// <summary>
        /// A readonly list of <see cref="IConversionNotificationListener"/> instances
        /// </summary>
        public IImmutableList<IConversionNotificationListener> NotificationListeners { get; }

        /// <summary>
        /// A readonly map of poco types to property names, indicating properties that should be ignored by the <see cref="IonConverters"/>
        /// when converting objects
        /// </summary>
        public IImmutableDictionary<Type, IImmutableSet<PropertyInfo>> IgnoredProperties { get; }

        /// <summary>
        /// 
        /// </summary>
        public NullValueBehavior NullValueBehavior { get; }

        /// <summary>
        /// 
        /// </summary>
        public DefaultValueBehavior DefaultValueBehavior { get; }


        internal ConversionOptions(
            NullValueBehavior nullValueBehavior,
            DefaultValueBehavior defaultValueBehavior,
            IEnumerable<IClrConverter> clrConverters,
            IEnumerable<IIonConverter> ionConverters,
            IEnumerable<IConversionNotificationListener> notificationListeners,
            IDictionary<Type, PropertyInfo[]> ignoredProperties)
        {
            NullValueBehavior = nullValueBehavior;
            DefaultValueBehavior = defaultValueBehavior;
            ClrConverters = clrConverters?.ToImmutableList() ?? throw new ArgumentNullException(nameof(clrConverters));
            IonConverters = ionConverters?.ToImmutableList() ?? throw new ArgumentNullException(nameof(ionConverters));
            NotificationListeners = notificationListeners.ToImmutableList() ?? throw new ArgumentNullException(nameof(notificationListeners));
            IgnoredProperties = ignoredProperties?
                .ToImmutableDictionary(
                    kvp => kvp.Key,
                    kvp => (IImmutableSet<PropertyInfo>)kvp.Value.ToImmutableHashSet())
                ?? throw new ArgumentNullException(nameof(ignoredProperties));
        }
    }

    public class ConversionOptionsBuilder
    {
        private readonly List<IClrConverter> clrConverters = new List<IClrConverter>();
        private readonly List<IIonConverter> ionConverters = new List<IIonConverter>();
        private readonly List<IConversionNotificationListener> conversionNotificationListeners = new List<IConversionNotificationListener>();
        private readonly Dictionary<Type, PropertyInfo[]> ignoredProperties = new Dictionary<Type, PropertyInfo[]>();
        private NullValueBehavior nullValueBehavior;
        private DefaultValueBehavior defaultValueBehavior;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ConversionOptionsBuilder WithClrConverters(params IClrConverter[] converters)
        {
            if (converters is null)
                throw new ArgumentNullException(nameof(converters));

            clrConverters.AddRange(
                converters.ThrowIfContainsNull(
                    new ArgumentNullException("null converters are forbidden")));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ConversionOptionsBuilder WithIonConverters(params IIonConverter[] converters)
        {
            if (converters is null)
                throw new ArgumentNullException(nameof(converters));

            ionConverters.AddRange(
                converters.ThrowIfContainsNull(
                    new ArgumentNullException("null converters are forbidden")));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listeners"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ConversionOptionsBuilder WithINotificationListeners(params IConversionNotificationListener[] listeners)
        {
            if (listeners is null)
                throw new ArgumentNullException(nameof(listeners));

            conversionNotificationListeners.AddRange(
                listeners.ThrowIfContainsNull(
                    new ArgumentNullException("null listeners are forbidden")));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultValueBehavior"></param>
        /// <returns></returns>
        public ConversionOptionsBuilder WithDefaultValueBehavior(DefaultValueBehavior defaultValueBehavior)
        {
            this.defaultValueBehavior = defaultValueBehavior;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nullValueBehavior"></param>
        /// <returns></returns>
        public ConversionOptionsBuilder WithNullValueBehavior(NullValueBehavior nullValueBehavior)
        {
            this.nullValueBehavior = nullValueBehavior;
            return this;
        }

        /// <summary>
        /// re/sets the map of ignored properties
        /// </summary>
        /// <param name="pocoType"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public ConversionOptionsBuilder WithIgnoredProperties(params PropertyInfo[] properties)
        {
            properties
                .ThrowIfContainsNull(new ArgumentNullException("null property-names are forbidden"))
                .GroupBy(property => property.DeclaringType ?? throw new ArgumentNullException(nameof(property)))
                .ForAll(group => ignoredProperties[group.Key] = group.ToArray());
            return this;
        }

        /// <summary>
        /// re/sets the map of ignored properties
        /// </summary>
        /// <param name="pocoType"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public ConversionOptionsBuilder WithIgnoredProperties(Type pocoType, params string[] propertyNames)
        {
            return propertyNames
                .ThrowIfContainsNull(new ArgumentNullException("null property-names are forbidden"))
                .Select(name => pocoType.GetProperty(name))
                .Where(property => property is not null)
                .ToArray()
                .ApplyTo(WithIgnoredProperties);
        }

        public static ConversionOptionsBuilder FromOptions(ConversionOptions options)
        {
            return ConversionOptionsBuilder
                .NewBuilder()
                .WithNullValueBehavior(options.NullValueBehavior)
                .WithDefaultValueBehavior(options.DefaultValueBehavior)
                .WithClrConverters(options.ClrConverters.ToArray())
                .WithINotificationListeners(options.NotificationListeners.ToArray())
                .WithIgnoredProperties(options.IgnoredProperties.SelectMany(kvp => kvp.Value).ToArray());            
        }

        public static ConversionOptionsBuilder NewBuilder() => new ConversionOptionsBuilder();

        private ConversionOptionsBuilder()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ConversionOptions Build()
        {
            return new ConversionOptions(
                nullValueBehavior,
                defaultValueBehavior,
                clrConverters,
                ionConverters,
                conversionNotificationListeners,
                ignoredProperties);
        }
    }
}
