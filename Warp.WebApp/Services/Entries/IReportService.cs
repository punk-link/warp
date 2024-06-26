﻿namespace Warp.WebApp.Services.Entries;

public interface IReportService
{
    public ValueTask<bool> Contains(Guid id, CancellationToken cancellationToken);
    public Task MarkAsReported(Guid id, CancellationToken cancellationToken);
}