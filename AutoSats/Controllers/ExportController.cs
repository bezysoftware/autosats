using AutoSats.Execution;
using AutoSats.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace AutoSats.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly IExchangeScheduler scheduler;

    public ExportController(IExchangeScheduler scheduler)
    {
        this.scheduler = scheduler;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ExportAsync(int id)
    {
        var events = await this.scheduler.GetScheduleEventsAsync(id);

        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<ExchangeEventMap>();

            csv.WriteRecords(events.Select(x => new BuyWithdrawal(x)));

            var contentType = "text/csv";
            var fileName = "export.csv";

            return File(Encoding.UTF8.GetBytes(writer.ToString()), contentType, fileName);
        }
    }
}