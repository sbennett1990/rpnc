/*
 * Copyright (c) 2020 Scott Bennett <scottb@fastmail.com>
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;

namespace RPNcompiler
{
/// <summary>
/// Generates y86 assembly for each of the supported instruction types.
/// </summary>
public class Generator
{
	/// <summary>
	/// Generate assembly code to duplicate the top value of the stack.
	/// Top value is popped off the stack then pushed twice.
	/// </summary>
	public string GenDup()
	{
		string asm = @"
	# [DUP]
	# ensure there is an argument on the stack to duplicate
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $1, %ecx
	subl %ecx, %edx
	jl stack_error		# goto stack_error if depth < 1

	# pop the top value
	popl %ebx

	# push the value twice to duplicate it
	pushl %ebx
	pushl %ebx

	# increment stack depth because there's a new entry
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $1, %ecx
	addl %ecx, %edx		# depth++
	rmmovl %edx, (%esi)	# store value
";
		return asm;
	}

	/// <summary>
	/// Generate assembly code to pop two values off the top of the
	/// stack, subtract them, and push the result.
	/// </summary>
	public string GenMinus()
	{
		string asm = @"
	# [MINUS]
	# ensure there are two arguments on the stack
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $2, %ecx
	subl %ecx, %edx
	jl stack_error		# goto stack_error if depth < 2

	# pop two values
	popl %ecx
	popl %ebx

	# subtract
	subl %ecx, %ebx

	# push result onto stack
	pushl %ebx

	# decrement stack depth by one
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $-1, %ecx
	addl %ecx, %edx		# depth--
	rmmovl %edx, (%esi)	# store value
";
		return asm;
	}

	/// <summary>
	/// Generate assembly code to pop two values off the top of the
	/// stack, add them, and push the result.
	/// </summary>
	public string GenPlus()
	{
		string asm = @"
	# [PLUS]
	# ensure there are two arguments on the stack
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $2, %ecx
	subl %ecx, %edx
	jl stack_error		# goto stack_error if depth < 2

	# pop two values
	popl %ecx
	popl %ebx

	# add
	addl %ecx, %ebx

	# push result onto stack
	pushl %ebx

	# decrement stack depth by one
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $-1, %ecx
	addl %ecx, %edx		# depth--
	rmmovl %edx, (%esi)	# store value
";
		return asm;
	}

	public string GenMultiply()
	{
		throw new NotImplementedException();
	}

	public string GenDivide()
	{
		throw new NotImplementedException();
	}

	public string GenSwap()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Generate assembly to push a number onto the stack.
	/// </summary>
	public string GenPush(string value)
	{
		if (!int.TryParse(value, out int i))
		{
			throw new Exception($"couldn't parse '{value}' into an int");
		}

		string asm = $@"
	# [PUSH]
	# push the number {i} onto the stack
	irmovl ${i}, %ecx
	pushl %ecx

	# increment stack depth because there's a new entry
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $1, %ecx
	addl %ecx, %edx		# depth++
	rmmovl %edx, (%esi)	# store value
";
		return asm;
	}
}
}
