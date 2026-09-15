// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.DotNet.Cli.Commands.New;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli.Alias;
using Microsoft.TemplateEngine.Edge.Settings;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal sealed class AliasAddCommand(Func<ParseResult, ITemplateEngineHost> hostBuilder, NewAliasCommandDefinitionBase definition)
        : BaseCommand<AliasAddCommandArgs, NewAliasCommandDefinitionBase>(hostBuilder, definition)
    {
        protected override Task<NewCommandStatus> ExecuteAsync(
            AliasAddCommandArgs args,
            IEngineEnvironmentSettings environmentSettings,
            TemplatePackageManager templatePackageManager,
            ParseResult parseResult,
            CancellationToken cancellationToken)
        {
            HashSet<string> reservedNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (Command command in args.RootCommand.Subcommands)
            {
                reservedNames.Add(command.Name);
                reservedNames.UnionWith(command.Aliases);
            }

            IReadOnlyList<ITemplateInfo> templates = await templatePackageManager.GetTemplatesAsync(cancellationToken).ConfigureAwait(false);
            foreach (ITemplateInfo template in templates)
            {
                reservedNames.UnionWith(template.ShortNameList);
            }

            return AliasSupport.ManipulateAlias(
                new AliasRegistry(environmentSettings),
                args.AliasName,
                args.AliasTokens,
                reservedNames);
        }

        protected override AliasAddCommandArgs ParseContext(ParseResult parseResult) => new(Definition, parseResult);
    }
}
