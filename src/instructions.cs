

using System;
using System.Collections.Generic;

namespace RPNcompiler
{
/// <summary>
/// Represents an operation that the compiler will generate code for.
/// </summary>
public sealed class InstructionType : IEquatable<InstructionType>
{
	/// <summary>
	/// Holds the type of instruction.
	/// </summary>
	public char Type { get; private set; }

	/// <summary>
	/// Will generate code to push a number onto the stack.
	///</summary>
	public static readonly InstructionType Push = Make(Instructions.Push);

	/// <summary>
	/// Will generate code to pop two values from the stack, add
	/// them together, then push the result onto the stack.
	/// </summary>
	public static readonly InstructionType Plus = Make(Instructions.Plus);

	/// <summary>
	/// Will generate code to pop two values from the stack, subtract
	/// them, then push the result onto the stack.
	/// </summary>
	public static readonly InstructionType Minus = Make(Instructions.Minus);

	/// <summary>
	/// Will generate code to duplicate the topmost value of the stack.
	/// </summary>
	public static readonly InstructionType Dup = Make(Instructions.Dup);

	/// <summary>
	/// Will generate code to swap the positions of the two values on
	/// the top of the stack.
	/// </summary>
	public static readonly InstructionType Swap = Make(Instructions.Swap);

	/// <summary>
	/// Determines whether this InstructionType is equal to another
	/// InstructionType.
	/// </summary>
	public bool Equals(InstructionType other)
	{
		if (other == null) {
			return false;
		}

		return Type.Equals(other.Type);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	/// <summary>
	/// Builder to create a new InstructionType.
	/// </summary>
	private static InstructionType Make(char t)
	{
		return new InstructionType {
			Type = t
		};
	}

	// A hack of an enum, since C# doesn't allow char type enums.
	private static class Instructions
	{
		public const char Push = 'P';
		public const char Plus = '+';
		public const char Minus = '-';
		public const char Dup = 'D';
		public const char Swap = 'S';
	}
}

/// <summary>
/// Instruction holds a single 'thing' that the compiler must generate
/// code for. (Value is only used when the 'thing' is a number to be
/// pushed on the stack.)
/// </summary>
public class Instruction
{
	/// <summary>
	/// Holds the type of instruction this object represents.
	/// </summary>
	public InstructionType Type { get; set; }

	/// <summary>
	/// Holds the value of a number to be pushed on the stack.
	/// </summary>
	public string Value { get; set; }
}
}
