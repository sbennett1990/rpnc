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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RPNcompiler
{
/// <summary>
/// RPNc: A simplistic compiler that takes a Reverse Polish Notation
/// expression and compiles it to y86 assembly.
/// </summary>
public static class RPNC
{
	public static void Main(string[] args)
	{
		// TODO: add support for input file and input on command line
		string rpnExpression = "2 1 -";

		// TODO: don't overwrite output file unless user specifies force (-f)
		string outputFile = "a.ys";
		if (File.Exists(outputFile)) {
			Console.WriteLine("warning: file will be overwritten: {0}",
			    outputFile);
			//Usage();
		}

		if (string.IsNullOrWhiteSpace(rpnExpression)) {
			Console.WriteLine("error: rpn expression is empty");
			Quit();
		}

		Compiler comp = new Compiler();
		string y86asm = comp.Compile(rpnExpression);

		File.WriteAllText(outputFile, y86asm);
	}

	/// <summary>
	/// Exit program with exit status 1.
	/// </summary>
	private static void Quit()
	{
		Environment.Exit(1);
	}

	/// <summary>
	/// Print usage information to the console and terminate the program
	/// with exist status 1.
	/// </summary>
	private static void Usage()
	{
		Console.WriteLine("usage: rpnc [-f] -i infile -o outfile");
		Environment.Exit(1);
	}
}
}
