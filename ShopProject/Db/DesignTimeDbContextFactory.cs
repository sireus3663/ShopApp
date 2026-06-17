using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json.Nodes;

namespace ShopProject.Db
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "AppConfig.json");
            if (!File.Exists(configPath))
            { 
                configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "AppConfig.json");
            }

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Не найден файл конфигурации AppConfig.json");
            }

            var jsonString = File.ReadAllText(configPath);
            var root = JsonNode.Parse(jsonString);
            var connectionString = root["ConnectionStrings"]?.ToString();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Строка подключения не найдена в AppConfig.json");
            }

            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}