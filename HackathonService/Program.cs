using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HackathonService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var services = host.Services;
            using (var scope = services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                

                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                if (!await context.ObjectTypes.AnyAsync())
                {
                    context.ObjectTypes.AddRange(
                        new ObjectType { Name = "Здание", Description = "Жилые и нежилые здания" },
                        new ObjectType { Name = "Сооружение",  Description = "Инженерные сооружения" },
                        new ObjectType { Name = "Образовательное учреждение", Description = "Школы, детские сады" }
                    );
                    await   context.SaveChangesAsync();
                }

                var objectService = scope.ServiceProvider.GetRequiredService<ObjectService>();
                var xmlService = scope.ServiceProvider.GetRequiredService<XmlImportService>();

                if (!System.IO.File.Exists("data.xml"))
                {
                    string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Objects>
  <Object TypeId=""1"">
    <Name>Торговый центр ""Европа""</Name>
    <Address>ул. Ленина, 15</Address>
    <Attribute Name=""floorCount"">5</Attribute>
    <Attribute Name=""area"">2500</Attribute>
    <Attribute Name=""hasParking"">true</Attribute>
  </Object>
  <Object TypeId=""1"">
    <Name>Жилой комплекс ""Солнечный""</Name>
    <Address>пр. Мира, 42</Address>
    <Attribute Name=""floorCount"">12</Attribute>
    <Attribute Name=""area"">8500</Attribute>
    <Attribute Name=""hasParking"">false</Attribute>
  </Object>
</Objects>";

                    System.IO.File.WriteAllText("data.xml", xml);
                }

                await ShowMenu(objectService, xmlService);
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=HackathonDB;Trusted_Connection=True;"));

                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<ObjectService>();
                    services.AddScoped<XmlImportService>();
                });

        static async Task ShowMenu(ObjectService objService, XmlImportService xmlService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1.  Показать все объекты");
                Console.WriteLine("2. Найти объект по названию");
                Console.WriteLine("3. Добавить новый объект");
                Console.WriteLine("4. Изменить объект");
                Console.WriteLine("5. Удалить объект");
                Console.WriteLine("6. Добавить поручение");
                Console.WriteLine("7. Выполнить поручение");
                Console.WriteLine("8. Импорт из XML");
                Console.WriteLine("9.  Статистика (Dashboard)");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": await ShowAllObjects(objService); break;
                    case "2": await SearchObjects(objService); break;
                    case "3": await AddObject(objService); break;
                    case "4": await UpdateObject(objService); break;
                    case "5": await DeleteObject(objService); break;
                    case "6":  await AddAssignment(objService); break;
                    case "7": await CompleteAssignment(objService); break;
                    case "8": await ImportXml(xmlService); break;
                    case "9": await ShowDashboard(objService); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор"); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        static async Task ShowAllObjects(ObjectService service)
        {
            var objects = await service.GetAllObjectsAsync();
            Console.WriteLine($"\nВсего объектов:  {objects.Count}");
            Console.WriteLine(new string('-',  80));
            
            foreach (var obj in objects)
            {
                Console.WriteLine($"ID: {obj.Id} | Название: {obj.Name} | Адрес: {obj.Address} | Статус: {obj.Status}");
                Console.WriteLine($"Тип: {obj.ObjectType?.Name} | Создан: {obj.CreatedBy}");
                
                if (obj.Assignments != null && obj.Assignments.Count > 0)
                {
                    var completed = obj.Assignments.Count(a => a.Status == "completed");
                    Console.WriteLine($"Поручений: {obj.Assignments.Count} (Выполнено: {completed})");
   
                }
                Console.WriteLine(new string('-', 80));
            }
        }



        static async Task SearchObjects(ObjectService service)
        {
            Console.Write("Введите текст для поиска: ");
            var search = Console.ReadLine();            
            var results = await service.SearchObjectsAsync(search);
            Console.WriteLine($"\nНайдено: {results.Count}");            
            foreach (var obj in results)
            {
                Console.WriteLine($"- {obj.Id}: {obj.Name} ({obj.Address})");
            }
        }
        static async Task AddObject(ObjectService service)
        {
            Console.Write("Введите ID типа объекта: ");
            var typeId = int.Parse(Console.ReadLine());            
            Console.Write("Введите название: ");
            var name = Console.ReadLine() ?? "Без названия";           
            Console.Write("Введите адрес: ");
            var address = Console.ReadLine() ?? "";            
            Console.Write("Введите ID пользователя: ");
            var userId = int.Parse(Console.ReadLine());
            var obj = await service.CreateObjectAsync(typeId, name, address, userId);
            Console.WriteLine($"\nОбъект создан! ID: {obj.Id}");
        }
        static async Task UpdateObject(ObjectService service)
        {
            Console.Write("Введите ID объекта: ");
            var id = int.Parse(Console.ReadLine());            
            Console.Write("Новое название (пусто - не менять): ");
            var name = Console.ReadLine();            
            Console.Write("Новый адрес (пусто - не менять): ");
            var address = Console.ReadLine();            
            Console.Write("Новый статус (active/in_progress/resolved): ");
            var status = Console.ReadLine();
            var obj = await service.UpdateObjectAsync(id, name, address, status, null);
            Console.WriteLine($"\nОбъект обновлен: {obj.Name}");
        }
        static async Task DeleteObject(ObjectService service)
        {
            Console.Write("Введите ID объекта для удаления: ");
            var id = int.Parse(Console.ReadLine());            
            await service.DeleteObjectAsync(id);
            Console.WriteLine("Объект удален ");
        }
        static async Task AddAssignment(ObjectService service)
        {
            Console.Write("Введите ID объекта: ");
            var objId = int.Parse(Console.ReadLine());           
            Console.Write("Заголовок поручения: ");
            var title = Console.ReadLine() ?? "Без заголовка";           
            Console.Write("Описание: ");
            var desc = Console.ReadLine() ?? "";          
            Console.Write("Ответственный: ");
            var responsible = Console.ReadLine() ?? "Не назначен";           
            Console.Write("ID назначившего: ");
            var assignedBy = int.Parse(Console.ReadLine());           
            Console.Write("Срок (ГГГГ-ММ-ДД): ");
            var due = DateTime.Parse(Console.ReadLine());
            var task = await service.CreateAssignmentAsync(objId, title, desc, responsible, assignedBy, due);
            Console.WriteLine($"\nПоручение создано! ID: {task.Id}");
        }
        static async Task  CompleteAssignment(ObjectService service)
        { 
            Console.Write("Введите ID поручения: ");
            var id = int.Parse(Console.ReadLine());
            
            var task = await service.CompleteAssignmentAsync(id);
            Console.WriteLine($"Поручение выполнено! Статус: {task.Status}");
        }
        static async Task ImportXml (XmlImportService service)
        {
            Console.Write("Введите  путь к XML файлу (по умолчанию data.xml): ");
            var path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
                path = "data.xml";
            
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Файл не найден");
                return;
            }
            var xml = System.IO.File.ReadAllText(path);
            Console.Write("Введите  ID пользователя: ");
            var userId = int.Parse Console.ReadLine());

            var objects = await service.ImportFromXmlAsync(xml, userId);
            Console.WriteLine($"\nИмпортировано объектов: {objects.Count}");
        }
        static async Task ShowDashboard(ObjectService service)
        {
            var stats = await service.GetDashboardStatsAsync();           
            Console.WriteLine("\n=== СТАТИСТИКА ===");
            Console.WriteLine($"Всего объектов: {stats.TotalObjects}");
            Console.WriteLine($"Всего поручений: {stats.TotalAssignments}");
            Console.WriteLine($"Выполнено поручений: {stats.CompletedAssignments}");
            Console.WriteLine($"Просрочено поручений:  {stats.OverdueAssignments}");
            Console.WriteLine($"Объектов в работе: {stats.InProgressObjects}");
            Console.WriteLine($"Процент выполнения: {stats.CompletionRate:F2}%");
        }
    }
}