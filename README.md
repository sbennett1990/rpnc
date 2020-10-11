# RPNc

RPNc{ompiler} can compile a Reversh Polish Notation ("RPN") expression into
y86 assembly.

Inspired by [math-compiler](https://github.com/skx/math-compiler) by
Steve Kemp.

## Build

RPNc is written in C# and targets .NET Core 3.1 (with plans to implement
my mono build system so it can run on mono on OpenBSD).

To build on Windows:
    PS C:\rpnc> cd src
    PS C:\rpnc\src> dotnet build

## Maths

RPNc can compile RPN expressions such as `9 1 +`. Expressions are limited to
integer operations: there is no floating point support because my toy
simulator (**yess**) only supports 32bit integers.

### Supported Operations

* Addition - `+`
* Subtraction - `-`

### Planned (But Not Yet Implemented) Operation Support

* Multiplication - `*`
* Division - `/` (integer division)
* Power - `^`
* Modulo - `%`
* And - `and` (bitwise)
* Xor - `xor` (bitwise)
* Duplication - `dup`
* Swap - `swap`

## Errors

RPNc attempts to detect and report some common errors at compile-time and
runtime.

### Compile-Time Errors

### Runtime Errors

Any runtime errors will be reported by setting codes in the `%esi` and
`%edi` registers. First, `%esi` will be set to _-1_ like so:

    `%esi: ffffffff`

Then, `%edi` will be set to one of the following error numbers:

* 0x01: EDIV - divide by zero
* 0x02: ESTACK - RPN stack does not have enough values for an operation
* 0x04: ESTACKFULL - RPN stack contains too many values at the end of
  program evaluation
