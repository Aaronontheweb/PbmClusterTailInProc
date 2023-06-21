using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using PbmClusterTailInProc.App;
using Microsoft.Extensions.Hosting;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .WithRemoting("localhost", 8080)
            .WithClustering(new ClusterOptions(){ Roles = new[]{ "app" }, SeedNodes = new[]{ "akka.tcp://MyActorSystem@localhost:8080" }})
            .WithActors((system, registry, resolver) =>
            {
                var helloActor = system.ActorOf(Props.Create(() => new HelloActor()), "hello-actor");
                registry.Register<HelloActor>(helloActor);
            })
            .WithActors((system, registry, resolver) =>
            {
                var timerActorProps =
                    resolver.Props<TimerActor>(); // uses Msft.Ext.DI to inject reference to helloActor
                var timerActor = system.ActorOf(timerActorProps, "timer-actor");
                registry.Register<TimerActor>(timerActor);
            });
    });
});

var host = hostBuilder.Build();

await host.RunAsync();