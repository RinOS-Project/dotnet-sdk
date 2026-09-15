// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.DotNet.Cli.Commands.New;

namespace Microsoft.TemplateEngine.Cli.Commands;

internal sealed class AliasShowCommandArgs : GlobalArgs
{
    internal AliasShowCommandArgs(NewAliasShowCommandDefinition definition, ParseResult parseResult)
        : base(parseResult)
    {
        AliasName = parseResult.GetValue(definition.AliasNameArgument);
    }

    internal string? AliasName { get; }
}
