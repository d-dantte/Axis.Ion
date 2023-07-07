﻿using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.IO
{
    public readonly struct IonPacket
    {
        private readonly IIonValue[] ionValues;

        public IonPacket(params IIonValue[] ionValues)
        {
            this.ionValues = ionValues
                .ThrowIfNull(new ArgumentNullException(nameof(ionValues)))
                .ToArray();
        }

        public IonPacket(IEnumerable<IIonValue> ionValues)
        : this(ionValues?.ToArray() ?? throw new ArgumentNullException(nameof(ionValues)))
        { }

        /// <summary>
        /// Returns the number of elements in the packet
        /// </summary>
        public int Count => ionValues.Length;

        /// <summary>
        /// Gets an array of the elements in the packet
        /// </summary>
        public IIonValue[] IonValues => ionValues.ToArray();
    }
}
