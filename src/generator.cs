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
	/// stack, multiply them, and push the result.
	/// </summary>
	public string GenMultiply()
	{
		string asm = @"
	# [MULTIPLY]
	# ensure there are two arguments on the stack
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $2, %ecx
	subl %ecx, %edx
	jl stack_error		# goto stack_error if depth < 2

	# pop two values
	popl %edx		# %edx = val1
	irmovl x, %ebx
	rmmovl %edx, (%ebx)	# x = val1
	popl %ecx		# %ecx = count = val2
	irmovl y, %ebx
	rmmovl %ecx, (%ebx)	# y = val2

	# figure out if either are negative numbers
	irmovl $1, %eax
	irmovl $0, %ecx
	andl %edx, %edx		# test x (val1) first
	je done_m102		# if (x == 0) goto done; // result will be 0
	cmovg %ecx, %eax	# else if (x > 0) %eax = 0
	mrmovl (%ebx), %edx	# %edx = y (val2)
	irmovl $1, %ebx
	andl %edx, %edx		# test y (val2)
	je done_m102		# if (y == 0) goto done; // result will be 0
	cmovg %ecx, %ebx	# else if (y > 0) %ebx = 0
	xorl %ebx, %eax		# if (a == 1 ^ b == 1) result is negative
	irmovl result_is_neg, %ebx
	rmmovl %eax, (%ebx)	# if result will be negative, result_is_neg = 1

	# multiply
	irmovl x, %ebx
	mrmovl (%ebx), %edx
	pushl %edx
	call Abs		# Abs(x)
	rmmovl %eax, (%ebx)	# x = Abs(x)
	irmovl y, %ebx
	mrmovl (%ebx), %ecx
	pushl %ecx
	call Abs		# Abs(y)
	rmmovl %eax, (%ebx)	# y = Abs(y)
	xorl %eax, %eax		# %eax = result = 0
loop_m100:
	addl %edx, %eax		# result += x
	irmovl $-1, %ebx
	addl %ebx, %ecx		# count--
	jne loop_m100

	# end of loop; negate result if (result_is_neg == 1)
	irmovl result_is_neg, %ebx
	mrmovl (%ebx), %ecx
	andl %ecx, %ecx
	je done_m103		# if (!result_is_neg) goto done
	irmovl $0, %edi		# this should be safe as %edi should normally be 0
	subl %eax, %edi
	rrmovl %edi, %eax	# result = result * -1
	xorl %edi, %edi		# zero %edi
	jmp done_m103
done_m102:
	# we have determined that result will be 0
	irmovl $0, %eax
done_m103:
	# push result onto stack
	pushl %eax
	xorl %eax, %eax		# zero %eax

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

	public string GenDivide()
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

	/// <summary>
	/// Generate assembly code to swap the positions of the top two values
	/// on the stack.
	/// </summary>
	public string GenSwap()
	{
		string asm = @"
	# [SWAP]
	# ensure there are two arguments on the stack
	mrmovl (%esi), %edx	# %edx = depth
	irmovl $2, %ecx
	subl %ecx, %edx
	jl stack_error		# goto stack_error if depth < 2

	# pop two values and push them in the reverse order
	popl %ecx
	popl %ebx
	pushl %ecx
	pushl %ebx

	# stack depth is unchanged
";
		return asm;
	}
}
}
