using Aml.Infrastructure;
using Aml.Simulator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var options = new DbContextOptionsBuilder<AmlDbContext>()
    .UseSqlServer(config.GetConnectionString("AmlDb"))
    .Options;

await using var db = new AmlDbContext(options);

await ReferenceDataSeeder.SeedAsync(db, CancellationToken.None);

var reference = await SimulatorReferenceData.LoadAsync(db, config["Aml:HomeCountryCode"]!, CancellationToken.None);

await GeneratorDataSeeder.SeedAsync(db, reference, CancellationToken.None);

Console.WriteLine("Data seeded");