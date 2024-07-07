using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace ChatRoom.SDK.Extension;

internal static class ConfiguratorExtension
{
    public static IConfigurator AddListTemplateCommand<TCommand>(this IConfigurator configurator)
        where TCommand : ListAvailableTemplatesCommand
    {
        configurator.AddCommand<TCommand>("list-templates")
            .WithDescription("List available templates.")
            .WithExample(["list-templates"]);

        return configurator;
    }
}
