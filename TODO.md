
### 2022-12-08
1. `ion-symbol` greedily swallows up 'null' as identifiers. This can be mitigated by creating a custom 
   terminal (`Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.ICustomTerminal`) for symbol-identifiers, that parses identifiers
   but excludes keywords such as "null, null.bool, null.int, etc".

2. 