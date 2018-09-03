var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

Task("Kill")
    .Description("Mata processos de dotnet/node")
    .Does(() => {
        var processes = new[] {"dotnet","node","chromedriver","iedriverserver"}
         .SelectMany(System.Diagnostics.Process.GetProcessesByName)
         .OrderBy(p => p.ProcessName);

        foreach (var p in processes)
        {
            try
            {
               Information($"Matando processo {p.ProcessName} Id:{p.Id}");
                p.Kill();
            }
            catch
            {
                Error($"Nao pode matar o processo {p.ProcessName} Id:{p.Id}");
            }
        }
    });


Task("Clean")
    .Does(() =>
{
	CleanDirectory(Directory("./Tests/bin") + Directory(configuration));
	Information("Limpeza dos projetos executada - Configuracao:" + configuration);
});

Task("NugetRestore")
	.IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore("./Icatu.Compra.sln");
		Information("Restauracao dos pacotes Nuget executada");
    });

Task("Build")
	.IsDependentOn("NugetRestore")
    .Does(() => 
    {
			MSBuild("./Icatu.Compra.sln",  
				new MSBuildSettings {
					Verbosity = Verbosity.Minimal,
					Configuration = configuration
				}
        );
		Information("Compilacao executada");
    });	

Task("Migrations")
	.IsDependentOn("Build")
	.Does(() =>
{
	ExecutarMigration();
});

Task("Recreate-Database")
	.Does(() =>
{
	DotNetCoreTool("./Icatu.Compra.Dados/Icatu.Compra.Dados.csproj", "ef database drop -f --context WebCoreContexto");
	Information("Remocao do banco WebCore executada");	

	ExecutarMigration();
});

Task("Tests-Unit")
    .IsDependentOn("Build")
    .Does(() =>
{
	    var project = "./Icatu.Compra.Testes.Unidade/Icatu.Compra.Testes.Unidade.csproj";
		DotNetCoreTest(
			project,
			new DotNetCoreTestSettings()
			{
			    Configuration = configuration,
			    NoBuild = true
			}
		);
		Information("Testes de Unidade executados");
});

Task("Tests-Integration")
    .IsDependentOn("Build")
    .Does(() =>
{
	    var project = "./Icatu.Compra.Testes.Integracao/Icatu.Compra.Testes.Integracao.csproj";
		DotNetCoreTest(
			project,
			new DotNetCoreTestSettings()
			{
			    Configuration = configuration,
			    NoBuild = true
			}
		);
		Information("Testes de Integracao executados");
});

Task("Tests-API")
    .IsDependentOn("Build")
    .Does(() =>
{
	    var project = "./Icatu.Compra.Testes.API/Icatu.Compra.Testes.API.csproj";
		DotNetCoreTest(
			project,
			new DotNetCoreTestSettings()
			{
			    Configuration = configuration,
			    NoBuild = true
			}
		);
		Information("Testes de API executados");
});

Task("Tests-Acceptance")
    .IsDependentOn("Build")
    .Does(() =>
{
	    var project = "./Icatu.Compra.Testes.Aceitacao/Icatu.Compra.Testes.Aceitacao.csproj";
		DotNetCoreTest(
			project,
			new DotNetCoreTestSettings()
			{
			    Configuration = configuration,
			    NoBuild = true
			}
		);
		Information("Testes de Aceitacao executados");
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
	    var projects = GetFiles("./Icatu.Compra.Testes.*/*.csproj");
        foreach(var project in projects)
        {
            DotNetCoreTest(
                project.FullPath,
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true
                });
        }
});

Task("Default")
	.IsDependentOn("Build")
	.Does(() =>
{
	DotNetCoreTool("./Icatu.Compra.Dados/Icatu.Compra.Dados.csproj", "ef database update --context WebCoreContexto");
	Information("Migration WebCore executada");
});

private void ExecutarMigration(){
	DotNetCoreTool("./Icatu.Compra.Dados/Icatu.Compra.Dados.csproj", "ef database update --context WebCoreContexto -v");
	Information("Migration WebCore executada");
}

RunTarget(target);
