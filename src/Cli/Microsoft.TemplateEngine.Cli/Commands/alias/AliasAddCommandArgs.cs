// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.DotNet.Cli.Commands.New;

namespace Microsoft.TemplateEngine.Cli.Commands;

internal sealed class AliasAddCommandArgs : GlobalArgs
{
    internal AliasAddCommandArgs(NewAliasCommandDefinitionBase definition, ParseResult parseResult)
        : base(parseResult)
    {
        if (definition is not NewAliasAddCommandDefinition addDefinition)
        {
            throw new ArgumentException("The alias add definition is required.", nameof(definition));
        }

        AliasName = parseResult.GetValue(addDefinition.AliasNameArgument);
        List<string> tokens = (parseResult.GetValue(addDefinition.AliasValueArgument) ?? Array.Empty<string>()).ToList();
        tokens.AddRange(parseResult.UnmatchedTokens);
        AliasTokens = tokens;
    }

    internal string? AliasName { get; }

    internal IReadOnlyList<string> AliasTokens { get; }
}
