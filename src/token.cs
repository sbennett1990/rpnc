

using System;
using System.Collections.Generic;

namespace RPNcompiler
{
/// <summary>
/// Represents an RPN token.
/// </summary>
public sealed class TokenType : IEquatable<TokenType>
{
	public string Type { get; private set; }

	public static readonly TokenType AND = Make(Tokens.AND);

	public static readonly TokenType DUP = Make(Tokens.DUP);

	public static readonly TokenType NUMBER = Make(Tokens.NUMBER);

	public static readonly TokenType PLUS = Make(Tokens.PLUS);

	public static readonly TokenType MINUS = Make(Tokens.MINUS);

	public static readonly TokenType ASTERISK = Make(Tokens.ASTERISK);

	public static readonly TokenType SWAP = Make(Tokens.SWAP);

	public static readonly TokenType XOR = Make(Tokens.XOR);

	/// <summary>
	/// Determines whether this TokenType object equals another
	/// TokenType object.
	/// </summary>
	public bool Equals(TokenType other)
	{
		if (other == null) {
			return false;
		}

		return string.Equals(Type, other.Type);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	/// <summary>
	/// Builder to create a new InstructionType.
	/// </summary>
	private static TokenType Make(string t)
	{
		return new TokenType {
			Type = t
		};
	}

	// A hack of an enum, since C# doesn't allow string type enums.
	private static class Tokens
	{
		public const string EOF = "EOF";
		public const string ERROR = "ERROR";
		public const string NUMBER = "NUMBER";
		public const string IDENT = "IDENT";

		/* integer operations */
		public const string PLUS = "+";
		public const string MINUS = "-";
		public const string ASTERISK = "*";
		public const string SLASH = "/";
		public const string MOD = "%";
		public const string POWER = "^";
		public const string FACTORIAL = "!";

		/* bitwise operations */
		public const string AND = "and";
		public const string XOR = "xor";

		/* stack operations */
		public const string DUP = "dup";
		public const string SWAP = "swap";
	}
}

/// <summary>
/// Token represents a lexer token.
/// </summary>
public class Token
{
	/// <summary>
	/// Holds the type of token this object represents.
	/// </summary>
	public TokenType Type { get; set; }

	/// <summary>
	/// Holds the literal number from the RPN expression.
	/// </summary>
	public string Literal { get; set; }

	/// <summary>
	/// Reserved keywords.
	/// </summary>
	private Dictionary<string, TokenType> keywords =
		new Dictionary<string, TokenType> {
			{ "and", TokenType.AND },
			{ "dup", TokenType.DUP },
			{ "swap", TokenType.SWAP },
			{ "xor", TokenType.XOR },
		};
}
}
