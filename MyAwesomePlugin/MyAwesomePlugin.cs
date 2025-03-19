using System;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using JetBrains.Annotations;

namespace ASFRemoveFreeLicenses;

#pragma warning disable CA1812 // ASF uses this class during runtime
[UsedImplicitly]
internal sealed class ASFRemoveFreeLicenses : IGitHubPluginUpdates {
	public string Name => nameof(ASFRemoveFreeLicenses);
	public string RepositoryName => "ItsJiro/ASFRemoveFreeLicenses";
	public Version Version => typeof(ASFRemoveFreeLicenses).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	private Timer _timer;


	public Task OnLoaded() {
		ASF.ArchiLogger.LogGenericInfo("Remove Free Games Plugin loaded!");
		_timer = new Timer(RemoveFreeGames, null, TimeSpan.Zero, TimeSpan.FromHours(1));
		return Task.CompletedTask;
	}

	private async void RemoveFreeGames(object state)
	{
		foreach (var bot in Bot.Bots?.Values)
		{
			var licenses = await bot.GetLicenses().ConfigureAwait(false);
			var freeLicenses = licenses?.Where(license => license.Type == ELicenseType.Complimentary).ToList();

			if (freeLicenses == null || !freeLicenses.Any())
			{
				ASF.ArchiLogger.LogGenericInfo($"No free licenses found for bot {bot.BotName}.");
				continue;
			}

			foreach (var license in freeLicenses)
			{
				var result = await bot.RemoveLicense(license.PackageID).ConfigureAwait(false);
				if (result)
				{
					ASF.ArchiLogger.LogGenericInfo($"Successfully removed free license {license.PackageID} from bot {bot.BotName}.");
				}
				else
				{
					ASF.ArchiLogger.LogGenericError($"Failed to remove free license {license.PackageID} from bot {bot.BotName}.");
				}
			}
		}
	}

	public Task OnUnloaded()
	{
		_timer?.Dispose();
		ASF.ArchiLogger.LogGenericInfo("Remove Free Games Plugin unloaded!");
		return Task.CompletedTask;
	}
}
#pragma warning restore CA1812 // ASF uses this class during runtime
