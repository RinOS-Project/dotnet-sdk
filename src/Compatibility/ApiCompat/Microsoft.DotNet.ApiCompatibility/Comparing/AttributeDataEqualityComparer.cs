// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.ApiCompatibility.Comparing
{
    /// <summary>
    /// Defines methods to support the comparison of <see cref="AttributeData"/> for equality.
    /// </summary>
    public sealed class AttributeDataEqualityComparer(IEqualityComparer<ISymbol> symbolEqualityComparer,
        IEqualityComparer<TypedConstant> typedConstantEqualityComparer) : IEqualityComparer<AttributeData>
    {
        /// <inheritdoc />
        public int GetHashCode([DisallowNull] AttributeData obj)
        {
            var hash = new HashCode();
            hash.Add(obj.AttributeClass is null ? 0 : symbolEqualityComparer.GetHashCode(obj.AttributeClass));

            foreach (var argument in obj.ConstructorArguments)
            {
                hash.Add(typedConstantEqualityComparer.GetHashCode(argument));
            }

            var namedArgumentComparer = new NamedArgumentComparer(typedConstantEqualityComparer);
            foreach (var argument in obj.NamedArguments)
            {
                hash.Add(namedArgumentComparer.GetHashCode(argument));
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public bool Equals(AttributeData? x, AttributeData? y)
        {
            if (x != null && y != null)
            {
                if (!symbolEqualityComparer.Equals(x.AttributeClass!, y.AttributeClass!))
                {
                    return false;
                }


                if (!Enumerable.SequenceEqual(x.ConstructorArguments, y.ConstructorArguments, typedConstantEqualityComparer))
                {
                    return false;
                }

                return Enumerable.SequenceEqual(x.NamedArguments, y.NamedArguments, new NamedArgumentComparer(typedConstantEqualityComparer));
            }

            return x == y;
        }
    }
}
