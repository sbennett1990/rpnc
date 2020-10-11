
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPNcompiler
{
/// <summary>
/// Core compiler, consisting of a 3-step process:
/// </summary>
public class Compiler
{
	/// <summary>
	/// The postfix expression that will be compiled.
	/// </summary>
	private string expression;

	/// <summary>
	/// The parsed expression which has been broken down into a series of tokens.
	/// Tokens are received from the lexer and are not modified.
	/// </summary>
	private List<Token> tokens;

	/// <summary>
	/// The virtual instructions (a.k.a. the intermediate form) that
	/// will be compiled into assembly.
	/// </summary>
	private List<Instruction> instructions;

	/// <summary>
	/// Convert the input RPN expression into a y86 assembly program.
	/// </summary>
	public string Compile(string rpnExpression)
	{
		expression = rpnExpression;

		Tokenize();
		Internalize();
		string y86asm = EmitCode();

		return y86asm;
	}


	private void Tokenize()
	{
		tokens = new List<Token>();

		/*
		 * TODO: Hard-coded test expression until I get a lexer working...
		 */
		tokens.Add(new Token { 
			Type = TokenType.NUMBER,
			Literal = "7"
		});
		tokens.Add(new Token { 
			Type = TokenType.NUMBER,
			Literal = "1"
		});
		tokens.Add(new Token {
			Type = TokenType.PLUS
		});
		tokens.Add(new Token {
			Type = TokenType.NUMBER,
			Literal = "3"
		});
		tokens.Add(new Token {
			Type = TokenType.MINUS
		});

		if (tokens.Count < 1) {
			throw new Exception("Tokenizing: the input expression appears to be empty");
		}

		if (!tokens.First().Type.Equals(TokenType.NUMBER)) {
			throw new Exception("Tokenizing: expected input expression to begin with a number");
		}

		if (tokens.Last().Type.Equals(TokenType.NUMBER)) {
			throw new Exception("Tokenizing: RPN expression cannot end with a number");
		}
	}

	/// <summary>
	/// Convert the series of tokens into an intermediate form.
	/// This is the "middle step" before generating assembly.
	/// </summary>
	private void Internalize()
	{
		instructions = new List<Instruction>();

		foreach (Token tok in tokens) {
			switch (tok.Type) {
			case TokenType t when t.Equals(TokenType.NUMBER):
				instructions.Add(new Instruction {
					Type = InstructionType.Push,
					Value = tok.Literal
				});
				break;
			case TokenType t when t.Equals(TokenType.DUP):
				instructions.Add(new Instruction {
					Type = InstructionType.Dup
				});
				break;
			case TokenType t when t.Equals(TokenType.MINUS):
				instructions.Add(new Instruction {
					Type = InstructionType.Minus
				});
				break;
			case TokenType t when t.Equals(TokenType.PLUS):
				instructions.Add(new Instruction {
					Type = InstructionType.Plus
				});
				break;
			case TokenType t when t.Equals(TokenType.SWAP):
				instructions.Add(new Instruction {
					Type = InstructionType.Swap
				});
				break;
			default:
				throw new Exception($"Tokens -> Instructions: unrecognized token type: {tok.Type.Type}");
			}
		}
	}

	/// <summary>
	/// Emit the y86 assembly program, joining a header, a footer,
	/// and our generated code.
	/// </summary>
	private string EmitCode()
	{
		Generator generator = new Generator();
		string header = CreateHeader();

		/*
		 * Walk the internal representation of the program and output
		 * a bit of assembly for each of the operator types.
		 */
		StringBuilder body = new StringBuilder();

		foreach (Instruction opr in instructions) {
			switch (opr.Type) {
			case InstructionType it when it.Equals(InstructionType.Dup):
				body.Append(generator.GenDup());
				break;
			case InstructionType it when it.Equals(InstructionType.Minus):
				body.Append(generator.GenMinus());
				break;
			case InstructionType it when it.Equals(InstructionType.Plus):
				body.Append(generator.GenPlus());
				break;
			case InstructionType it when it.Equals(InstructionType.Push):
				body.Append(generator.GenPush(opr.Value));
				break;
			case InstructionType it when it.Equals(InstructionType.Swap):
				body.Append(generator.GenSwap());
				break;
			default:
				throw new Exception($"Instructions -> Assembly: unrecognized instruction type: {opr.Type.Type}");
			}
		}

		string footer = CreateFooter();

		return header + body.ToString() + footer;
	}

	/// <summary>
	/// Make the header section of the y86 program.
	/// </summary>
	private static string CreateHeader()
	{
		string asm = @"#
# This program was compiled using RPNcompiler, by Scott Bennett
#
# Execution begins at address 0
	.pos 0
init:	irmovl Stack, %esp	# Set up stack pointer
	irmovl Stack, %ebp	# Set up base pointer
	jmp Main		# Execute main program

# Data section
	.align 4
depth:	.long 0x3		# Keeps track of the RPN stack depth

EDIV:		.long 0x01	# Divide by 0 errno
ESTACK:		.long 0x02	# Depth of RPN stack too shallow errno
ESTACKFULL:	.long 0x04	# Depth of RPN stack too high errno

#
# Main function
#
Main:
	# The RPN stack is initially empty, so initialize depth to zero
	irmovl depth, %esi	# %esi holds the address of depth
	mrmovl (%esi), %edx	# %edx holds the value of depth
	xorl %edx, %edx		# zero the register
	rmmovl %edx, (%esi)	# depth = 0
	xorl %edi, %edi		# zero %edi since it holds error codes
";
		return asm;
	}

	/// <summary>
	/// Make the footer section of the y86 program, which stores the
	/// result in register %eax and prints the register contents.
	/// </summary>
	private static string CreateFooter()
	{
		// TODO: check the Stack position

		string asm = @"
# Footer section:

	# Check that only one number is left on the stack
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $1, %ecx
	subl %ecx, %edx
	jne stack_too_full	# goto stack_too_full if depth != 1

	# Pop the result off the stack and return in %eax
	popl %eax

	# print result (only the registers)
	call $5	# dump
	halt

# Error conditions section:

#
# Division by 0 was attempted
#
divide_by_zero:
	irmovl EDIV, %ebx	# %ebx holds address of EDIV errno
	jmp set_code_and_exit

#
# Not enough operands on the RPN stack for an operation
# (For example: '1 +', or '3 2 % -')
#
stack_error:
	irmovl ESTACK, %ebx	# %ebx holds address of ESTACK errno
	jmp set_code_and_exit

#
# RPN stack has too many numbers on it at the end of a program
# (For example: '3 2 1 +')
#
stack_too_full:
	irmovl ESTACKFULL, %ebx	# %ebx holds address of ESTACKFULL errno
	jmp set_code_and_exit

#
# Store the error code in %edi and terminate
#
set_code_and_exit:
	mrmovl (%ebx), %edi	# %edi holds error codes
	xorl %ebx, %ebx		# clear %ebx
	irmovl $-1, %esi	# set %esi to -1 to also indicate error
	call $1	# dump
	halt

	# Stack starts at the highest memory location
	.pos 0xffc
Stack:
";
		return asm;
	}
}
}
