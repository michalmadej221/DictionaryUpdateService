﻿using DictionaryService;
using DictionaryService.DictManagement;
using Serilog;

namespace DictionaryUpdate.ApiHosting;

public class Worker : BackgroundService
{
    private bool NeedUpdate = true;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Log.Information("Worker! Checking DateTime {hour}", DateTimeOffset.Now.Hour);
            if (DateTimeOffset.Now.Hour == AppConfig.UpdateHour && NeedUpdate)
            {

                var dictsToUpdate = new List<Dictionary>();
                var updateList = DictCompare.Compare();

                foreach (var dict in updateList)
                    if (dict.UpdateNeeded)
                        if (dict.AutoUpdate)
                            dictsToUpdate.Add(dict);

                NeedUpdate = false;
                Log.Information("Worker started downloading");

                foreach (var dictionary in dictsToUpdate)
                    dictionary.UpdateDictionary();

                Log.Information("Worker downloading finished");

            }
            else if (DateTimeOffset.Now.Hour != AppConfig.UpdateHour && !NeedUpdate)
                NeedUpdate = true;

            await Task.Delay(600000, stoppingToken);
        }
    }
}
