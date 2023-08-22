using CliFx;

return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
