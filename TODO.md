
### 2022-12-08
1. `ion-symbol` greedily swallows up 'null' as identifiers. This can be mitigated by creating a custom 
   terminal (`Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.ICustomTerminal`) for symbol-identifiers, that parses identifiers
   but excludes keywords such as "null, null.bool, null.int, etc".
   [DONE]

2. Include the following: [DONE]
	1. ML-SQD-String: a delimited string recognizer that recognizes strings that contain new-lines, and are enclosed in single-quotes
	2. SL-SQD-String: a delimited string recognizer that recognizes strings that do not contain new-lines (\n or \r), and are enclosed
	   in single-quotes
	3. SL-DQD-String: a delimited string recognizer that recognizes strings that do not contain new-lines (\n or \r), and are enclosed
	   in double-quotes
3. Test the string and clob recognizers again. [DONE]
4. Add support for symbol ids again with the following implementation: [DONE]

   WRITING
   1. Maintain a hash-list (indexed unique items) of symbols (quoted and identifier) that is visible to the entire Write operation.
   2. Each time a quoted/identifier symbol is encountered, no matter how deep in the ion graph, an attemp is made to add the symbol
      to the hash-list.
   3. If the symbol isn't present in the hash-list, add it, and write the symbol in its raw form to the stream.
   4. If the symbol is already present in the hash-list, write the symbol in its id form to the stream; the id will be the index in
      the hash-list

   The above ensures that each id form of a symbol encountered in the stream is guaranteed to have been previously encountered in its
   raw form.

   READING
   1. Maintain a hash-list (indexed unique items) of symbols (quoted and identifier) that is visible to the entire Read operation.
   2. Each time a raw quoted/identifier symbol is encountered in the stream, deserialize and add it to the hash-list.
   3. Each time a symbol id is encountered, use the id to find it in the hash-list.

   Note that when added to the hash-list, or tested against the hash-list:
   1. annotations are striped away from the symbol.
   2. null symbols are NEVER added to the table, as they are already trivial to serialize


### 2023-02-05
1. Modify symbol parsing/initialization as follows: [DONE]
	1. using ITextSymbol.Parse(...), the order of parsing is `Identifier` -> `Quoted symbol`.
	2. Quoted Symbols can be initialized with text that isn't wrapped in the single-quote (') symbol.
	3. Allowed characters for quoted symbols include:
		1. '\u0020'..'\u0026' i.e, no C1 control characters and no U+0027 single quote
		2. '\u0028'..'\u005B' i.e, no U+005C backslash
		3. '\u005D'..'\uFFFF' i.e, should be up to U+10FFFF
	4. While initializing the quoted symbol, the start and end single-quote symbol is trimmed off (if present), and the text alone is stored.
2. Rename the `Axis.Ion.IO.Binary` namespace to `Axis.Ion.IO.Axion` [Done]
3. Repurpose the `Axis.Ion.IO.Binary` namespace for the original Amazon Ion Binary format implementation.