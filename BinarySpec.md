# Ion Custom Binary Format
This document aims to flesh out the specification for the binary representation of the ion data types


## Contents
1. [Introduction](#Introduction)
2. [Annotations](#Annotations)
3. [Ion-Null](#ion-null)
4. [Ion-Bool](#ion-bool)
5. [Ion-Int](#ion-int)
6. [Ion-Decimal](#ion-decimal)
7. [Ion-Float](#ion-float)
8. [Ion-Timestamp](#ion-timestamp)
9. [Ion-String](#ion-string)
10. [Ion-Symbol](#ion-symbol)
11. [Ion-Blob](#ion-blob)
12. [Ion-Clob](#ion-clob)
13. [Ion-List](#ion-list)
14. [Ion-Sexp](#ion-sexp)
15. [Ion-Struct](#ion-struct)
16. [Apendix](#Apendix)


## Introduction
The binary format for ion data described here is chosen to be similar to the 
(official)[https://amazon-ion.github.io/ion-docs/docs/binary.html] format, but with some differences
that facilitate ease/simplicity of procesing.

Generally speaking, the first byte of each ion-value, henceforth called the type-metadata, represents
it's _type_: since ion only supports 13 distinct types, this byte is more than sufficient to represent
13 distinct values: 1 - 13. For some ion values (bool and null), a single byte is usually enough to encapsulate
the entire data.

The first 4 bits (index 0 - 3) are reserved to represent actual type identifiers, bit 5 (index 4) is reserved
for indicating if annotations exist on the value, while the remaining 3 bits (index 5 - 7) are left for each
type to use as it pleases.

Following the first byte either byte-groups that represent annotations, or the payload for the ion-value.


Note: in the binary notations that follow, a  '.' indicates that the bit in that position is ignored.


## Annotations

### Type-Metadata
- `[...1-....]`

### Description
Annotations are not actual ion types, they exist to decorate ion values. When present, a bit in the
type-metadata is flipped on. When this happens, the next byte(s) after the type-metadata represents the
_annotation count_: a [`var-byte`](#var-byte) indicating how many annotations are present on the value;
each annotation itself being a symbol (quoted/identifier). 

## Ion-Null

### Type-Metadata
- `[....-0001]`
- 1 (dec)
- 0x1 (hex)

### Description
The type-metadata alone is enough to represent the ion-null. Extra information is only present if, for example,
annotations are present on the null value.


## Ion-Bool

### Type-Metadata
- `[....-0010]`
- 2 (dec)
- 0x2 (hex)

### Description
Like the null type, the type-metadata byte is sufficient for encapsulating all instances of the bool value.

- true: `[.1..-0010]`
- false: `[.0..-0010]`
- null: `[..1.-0010]`


## Ion-Int

### Type-Metadata
- `[....-0011]`
- 3 (dec)
- 0x3 (hex)

### Custom-Metadata
- null: `[..1.-0011]`
- Int8: `[00..-0011]`
- Int16: `[01..-0011]`
- Int32: `[10..-0011]`
- Intxx: `[11..-0011]`

### Description
Integers require additional bytes for their data to be represented. This will usually follow the annotations
if present, or follow the type-metadata.

Ion supports arbitrary-length integers, so special consideration is taken to cater for this. 

### Examples
1. To represent the value '42' as an Int8 value, we need 2 bytes:
   - 00 [00..-0011]
   - 01 [0010-1010]
2. To represent the value '1228' as an Int16 value, we need 3 bytes:
   - 00 [01..-0011]
   - 01 [1100-1100]
   - 02 [0000-0100]
3. To represent the value '86443187' as an Int32 value, we need 5 bytes:
   - 00 [10..-0011]
   - 01 [1011-0011]
   - 02 [0000-0100]
   - 03 [0010-0111]
   - 04 [0000-0101]
4. To represent arbitrarily large integers, e.g '9223372036854775807', we need 11 bytes:
   - 00 [11..-0011]
   - 01 [0000-1010] - [`var-byte`](var-byte) byte-count (10)
   - 02 [1110-0011]
   - 03 [1111-0010]
   - 04 [1111-1111]
   - 05 [1111-1111]
   - 06 [1111-1111]
   - 07 [1111-1111]
   - 08 [1111-1111]
   - 09 [1111-1111]
   - 10 [1000-0111]
   - 11 [0001-0011]

The special byte-count byte can extend beyond a single byte - in the rare/unlikely situation where an integer
needs more than 4 billion bytes to be represented, the bit at position 7 (overflow bit) is flipped to a 1 `[1...-...]`,
indicating that another byte follows for which combining it with the previous byte, converts to an integer
that indicates the byte count. Note that combining these byte-count values must first remove the overflow bit.
See [var-byte](#var-byte).


## Ion-Decimal

### Type-Metadata
- `[....-0100]`
- 4 (dec)
- 0x4 (hex)

### Custom-Metadata
- null: `[..1.-0100]`

### Description
The binary representation for decimals here is a straightforward adaptation of the c# binary representation for decimal,
ergo 16 bytes of data. This means decimal values have a fixed 17byte size.


## Ion-Float

### Type-Metadata
- `[....-0101]`
- 5 (dec)
- 0x5 (hex)

### Custom-Metadata
- null: `[..1.-0101]`
- regular float: `[....-0101]`
- nan: `[.1..-0101]`
- pinf: `[1...-0101]`
- ninf: `[11..-0101]`

### Description
As with [decimal](#Ion-Decimal) above, floats have a fixed-sized, striaghtfowrard representation in binary. Floats need
total of 9 bytes: 1 for type-metadata, 8 for float data.
For the values - `nan`, `positive-infinity`, `negative-infinity`, the following conventions are used to store them within
the custom-metadata:
1. (00) - represents a regular float, meaning all 9 bytes are used for storage.
2. (01) - represents a "nan" value. In this state, Only the metadata byte is used.
3. (10) - represents a "positive infinity" value. Same as above: only the metadata is used.
4. (11) - represents a "negative infinity" value. Same as above applies.


## Ion-Timestamp

### Type-Metadata
- `[....-0110]`
- 6 (dec)
- 0x6 (hex)

### Custom-Metadata
- null: `[..1.-0110]`

### Description
The timestamp is made of 7 components, each with their own binary representation, the first 2 of which are mandatory.
In practice, however, there are actually only 5 components, because the `Hour`, `Minute` and `Second` components
are stored as a unit of seconds, using 3 bytes.

The timestamp's first data byte holds information regarding what optional components are present (or absent). This is
done using the first (lower-end) 3 bits. The remaining bits are reserved for the following use.

The 4th bit (index 3) is reserved for use by the `HMS` (Hour, Minute, Second) component. There are a total of _86,400_
seconds in a day, this needs exactly 17 bits to be represented. Since 3 bytes holds 24 bits, there'll be a total of 7
unused bits in the 3rd byte if 3 whole bytes are used to store the information. To mitigate this, the 17th bit is
"borrowed" from the 4th bit of the component-byte.

Bits 5 - 8 (index 4 - 7) are reserved for the "ticks" component of the timestamp. The ticks component requires 20 bits
to store it's information: this is 8 + 8 + 4, i.e 2 bytes, and 4 bits. The 4 bits are "borrowed" from the component
byte.

The following lists the components in the order they appear:
1. Components flags: 1 byte
2. Year: The year component is represented by a [var-byte](#var-byte)
3. Month: represented by 1 byte.
4. Day: represented by 1 optional byte
5. HMS (hour, minute, seconds): 17 optional bits (2 bytes, and one reserved bit)
8. Ticks/Sub-second: represented by 1 optional int (3 bytes). 1 second = 10,000,000 ticks.

Writing/reading the timestamp uses the component flags to figure, in order, what component is found at what position.


## Ion-String

### Type-Metadata
- `[....-0111]`
- 7 (dec)
- 0x7 (hex)

### Custom-Metadata
- null: `[..1.-0111]`

### Description
String data is stored as unicode - ie, 2 bytes per character; however, a string-count component is used to signify
how many characters (groups of 2 bytes) the string contains. The string-count comes right after the type-metadata,
and is represented as a `var-byte`.


## Ion-Symbols

### Type-Metadata
- `[....-1000]` (operator)
- 8 (dec)
- 0x8 (hex)

- `[....-1001]` (identifier)
- 9 (dec)
- 0x9 (hex)

- `[....-1010]` (quoted symbol)
- 10 (dec)
- 0xA (hex)

### Custom-Metadata
- null: `[..1.-....]`
- raw: `[00..-....]`
- Int8 id: `[01..-....]`
- Int16 id: `[10..-....]`
- Intxx id: `[11..-....]
- 1-char-operator: `[00..-....]`
- 2-char-operator: `[01..-....]`
- 3-char-operator: `[10..-....]`
- x-char-operator: `[11..-....]`

### Description
Symbols in binary have 4 states (listed in the metadata section).
1. Operand: In this form, the operator symbol is represented by a single byte. There are a total of 19 operator symbols
   supported by ion. Each one is represented by it's ordinal value:
   1.  Exclamation
   2.  Hash       
   3.  Percent    
   4.  Ampersand  
   5.  Star       
   6.  Plus       
   7.  Minus      
   8.  Dot        
   9.  FSlash     
   10. SColon     
   11. Less       
   12. Equals     
   13. Greater    
   14. QMark      
   15. At         
   16. Caret      
   17. BTick      
   18. Pipe       
   19. Tilde 
   Operators are a sequence of one or more or the above characters, and so are stored using a similar technique as [ion-int](#ion-int).
   The symbols are represented as a single ascii byte, and stored contiguously with the char-count represented using the custom-metadata,
   or a `var-int`.
2. Quoted symbol: In this form, following the type-metadata is a series of `var-bytes` that represent the number of
   unicode characters present in the symbol. Beyond the count, are the bytes for the unicode characters. Note that
   the quotes aren't serialized, as they are implied by the type itself
3. Identifier: Similar to `quoted symbol` except for allowing only a limited set of characters.
   See [here](https://amazon-ion.github.io/ion-docs/docs/spec.html#symbol).
4. ID: The ID state exists when a quoted/identifier symbol has extra information stored in the custom-metadata section
   of the metadata byte. The information here represents the number of bytes needed to store the value of the symbol's
   ID.
	

## Ion Blob

### Type-Metadata
- `[....-1011]`
- 11 (dec)
- 0xB (hex)

### Custom-Metadata
- null: `[..1.-1001]`

### Description
Regular variable byte stream. Following the type-metadata, `var-byte` values that represent the number of expected bytes.
Following the `var-bytes` are the actual byte array
	

## Ion Clob

### Type-Metadata
- `[....-1100]`
- 12 (dec)
- 0xC (hex)

### Custom-Metadata
- null: `[..1.-1010]`

### Description
Same as [ion-blob](#ion-blob) above, except that the bytes represent unicode characters.


## Ion List

### Type-Metadata
- `[....-1101]`
- 13 (dec)
- 0xD (hex)

### Custom-Metadata
- null: `[..1.-1011]`

### Description
The list is similar to the [ion-blob](#ion-blob), except that the `var-byte` count represents number of items in the list.
Each item is an ion-value.


## Ion Sexp

### Type-Metadata
- `[....-1110]`
- 14 (dec)
- 0xE (hex)

### Custom-Metadata
- null: `[..1.-1100]`

### Description
The sexp is exactly the same as to the [ion-list](#ion-list), except that operand symbols are also allowed as items.


## Ion Struct

### Type-Metadata
- `[....-1111]`
- 15 (dec)
- 0xF (hex)

### Custom-Metadata
- null: `[..1.-1101]`

### Description
The list is similar to the [ion-list](#ion-list); that the `var-byte` count represents number of entries in the struct.
Each entry is a key-value pair, with the key being either a quoted symbol, or an identifier, immediately followed by an ion-value.



## Apendix

### var-byte
A variable byte binary representation. This is a regular 1-byte integer number, except that the sign-bit now represents
'overflow': if the bit is set, it means another [var-byte](#var-byte) value follows. Reading the collection of `var-byte`
data requires removing all the overflow bits, and concatenating the remaining bits.