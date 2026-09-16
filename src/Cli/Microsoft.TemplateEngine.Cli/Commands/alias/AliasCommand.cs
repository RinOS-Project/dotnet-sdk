// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.DotNet.Cli.Commands.New;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli.Alias;
using Microsoft.TemplateEngine.Edge.Settings;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal sealed class AliasCommand(Func<ParseResult, ITemplateEngineHost> hostBuilder, NewAliasCommandDefinition definition)
        : BaseCommand<AliasCommandArgs, NewAliasCommandDefinition>(hostBuilder, definition)
    {
        protected override Task<NewCommandStatus> ExecuteAsync(
            AliasCommandArgs args,
            IEngineEnvironmentSettings environmentSettings,
            TemplatePackageManager templatePackageManager,
            ParseResult parseResult,
            CancellationToken cancellationToken)
            => Task.FromResult(AliasSupport.DisplayAliasValues(
                environmentSettings,
                new AliasRegistry(environmentSettings),
                aliasName: null,
                NewAliasCommandDefinition.Name));

        protected override AliasCommandArgs ParseContext(ParseResult parseResult) => new(parseResult);
    }
}
