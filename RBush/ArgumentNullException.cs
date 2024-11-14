#if !NET6_0_OR_GREATER
#pragma warning disable IDE0005 // Using directive is unnecessary.
global using ArgumentNullException = RBush.ArgumentNullException;
#pragma warning restore IDE0005 // Using directive is unnecessary.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RBush;

[Browsable(false)]
[ExcludeFromCodeCoverage]
internal static class ArgumentNullException
{
	/// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
	/// <param name="argument">The reference type argument to validate as non-null.</param>
	/// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
	public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
	{
		if (argument is null)
			Throw(paramName);
	}

	[DoesNotReturn]
	private static void Throw(string? paramName) =>
		throw new System.ArgumentNullException(paramName);
}
#endif
