using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Antijank {

  [PublicAPI]
  public class AppDomainManager : System.AppDomainManager {

    [DllImport("kernel32")]
    private static extern bool AllocConsole();

    public override void InitializeNewDomain(AppDomainSetup appDomainInfo) {
      if (AllocConsole())
        Console.WriteLine("Diagnostic console allocated.");

      Trace.Listeners.Add(new ConsoleTraceListener(true));

      base.InitializeNewDomain(appDomainInfo);

      AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => {
        Console.Write("Loaded ");
        Console.WriteLine(args.LoadedAssembly.GetName().ToString());

        var name = args.LoadedAssembly.GetName().Name;
        if (name != "TaleWorlds.MountAndBlade.Launcher"
          && name != "TaleWorlds.Library"
          && name != "ManagedStarter")
          return;

        AssemblyResolver.Init();

        switch (name) {
          case "TaleWorlds.Library":
            CustomDebugManager.Init();
            break;
          case "TaleWorlds.MountAndBlade.Launcher":
            LoaderPatch.Init();
            break;
        }
      };
    }

  }

}