using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HackathonService
{
    public class ObjectService
    {
        private readonly IRepository<ControlObject> _objectRepo;
        private readonly IRepository<Attribute> _attrRepo;
        private readonly IRepository<AttributeValue> _valueRepo;
        private readonly IRepository<Assignment> _assignRepo;
        private readonly AppDbContext _context;

        public ObjectService(
            IRepository<ControlObject> objectRepo,
            IRepository<Attribute> attrRepo,
            IRepository<AttributeValue> valueRepo,
            IRepository<Assignment> assignRepo,
            AppDbContext context)
        {
            _objectRepo = objectRepo;
            _attrRepo = attrRepo;
            _valueRepo = valueRepo;
            _assignRepo = assignRepo;
            _context = context;
        }

        public async Task<ControlObject> GetObjectAsync(int id)
        {
            return await _context.ControlObjects
                .Include(o => o.ObjectType)
                .Include(o => o.AttributeValues)
                    .ThenInclude(v => v.Attribute)
                .Include(o => o.Assignments)
                .Include(o => o.Documents)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<List<ControlObject>> GetAllObjectsAsync()
        {
            return await _context.ControlObjects
                .Include(o => o.ObjectType)
                .Include(o => o.AttributeValues)
                    .ThenInclude(v => v.Attribute)
                .Where(o => !o.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<ControlObject>> SearchObjectsAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return await GetAllObjectsAsync();

            return await _context.ControlObjects
                .Include(o => o.ObjectType)
                .Include(o => o.AttributeValues)
                    .ThenInclude(v => v.Attribute)
                .Where(o => !o.IsDeleted &&
                    (o.Name.Contains(search) ||
                     o.Address.Contains(search) ||
                     o.ObjectType.Name.Contains(search) ||
                     o.AttributeValues.Any(v =>
                         v.ValueText != null && v.ValueText.Contains(search))))
                .ToListAsync();
        }

        public async Task<ControlObject> CreateObjectAsync(int typeId, string name, string address, int userId)
        {
            var obj = new ControlObject
            {
                ObjectTypeId = typeId,
                Name = name ?? "Без названия",
                Address = address ?? "",
                Status = "active",
                CreatedBy = userId
            };

            return await _objectRepo.AddAsync(obj);
        }

        public async Task<ControlObject> CreateObjectWithAttributesAsync(int typeId, string name, string address, int userId, Dictionary<string, object> attributes)
        {
            var obj = await CreateObjectAsync(typeId, name, address, userId);

            if (attributes != null)
            {
                var attrs = await _attrRepo.FindAsync(a => a.ObjectTypeId == typeId);

                foreach (var attr in attrs)
                {
                    if (attributes.TryGetValue(attr.Name, out var value))
                    {
                        var val = new AttributeValue
                        {
                            ObjectId = obj.Id,
                            AttributeId = attr.Id
                        };

                        SetAttributeValue(val, attr.FieldType, value);
                        await _valueRepo.AddAsync(val);
                    }
                }
            }

            return await GetObjectAsync(obj.Id);
        }

        public async Task<ControlObject> UpdateObjectAsync(int id, string name, string address, string status, Dictionary<string, object> attributes)
        {
            var obj = await _context.ControlObjects
                .Include(o => o.AttributeValues)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (obj == null)
                throw new Exception("Object not found");

            if (!string.IsNullOrEmpty(name))
                obj.Name = name;

            if (!string.IsNullOrEmpty(address))
                obj.Address = address;

            obj.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(status))
                obj.Status = status;

            await _objectRepo.UpdateAsync(obj);

            if (attributes != null)
            {
                var attrs = await _attrRepo.FindAsync(a => a.ObjectTypeId == obj.ObjectTypeId);

                foreach (var attr in attrs)
                {
                    if (attributes.TryGetValue(attr.Name, out var value))
                    {
                        var val = obj.AttributeValues.FirstOrDefault(v => v.AttributeId == attr.Id);

                        if (val != null)
                        {
                            SetAttributeValue(val, attr.FieldType, value);
                            await _valueRepo.UpdateAsync(val);
                        }
                        else
                        {
                            var newVal = new AttributeValue
                            {
                                ObjectId = obj.Id,
                                AttributeId = attr.Id
                            };
                            SetAttributeValue(newVal, attr.FieldType, value);
                            await _valueRepo.AddAsync(newVal);
                        }
                    }
                }
            }

            return await GetObjectAsync(id);
        }

        public async Task DeleteObjectAsync(int id)
        {
            var obj = await _objectRepo.GetByIdAsync(id);
            if (obj != null)
            {
                obj.IsDeleted = true;
                obj.UpdatedAt = DateTime.Now;
                await _objectRepo.UpdateAsync(obj);
            }
        }

        public async Task<Assignment> CreateAssignmentAsync(int objectId, string title, string description, string responsible, int assignedBy, DateTime dueDate)
        {
            var task = new Assignment
            {
                ObjectId = objectId,
                Title = title ?? "Без заголовка",
                Description = description ?? "",
                ResponsiblePerson = responsible ?? "Не назначен",
                AssignedBy = assignedBy,
                DueDate = dueDate,
                Status = "assigned"
            };

            return await _assignRepo.AddAsync(task);
        }

        public async Task<Assignment> CompleteAssignmentAsync(int id)
        {
            var task = await _assignRepo.GetByIdAsync(id);
            if (task == null)
                throw new Exception("Assignment not found");

            task.Status = "completed";
            task.CompletedDate = DateTime.Now;
            task.UpdatedAt = DateTime.Now;
            await _assignRepo.UpdateAsync(task);
            return task;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var totalObjects = await _objectRepo.CountAsync();
            var totalTasks = await _assignRepo.CountAsync();
            var completedTasks = await _assignRepo.CountAsync(a => a.Status == "completed");
            var overdueTasks = await _assignRepo.CountAsync(a => a.DueDate < DateTime.Now && a.Status != "completed");
            var inProgress = await _objectRepo.CountAsync(o => o.Status == "in_progress");

            return new DashboardStats
            {
                TotalObjects = totalObjects,
                TotalAssignments = totalTasks,
                CompletedAssignments = completedTasks,
                OverdueAssignments = overdueTasks,
                InProgressObjects = inProgress,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0
            };
        }

        private void SetAttributeValue(AttributeValue attrValue, string fieldType, object value)
        {
            if (value == null) return;

            switch (fieldType.ToLower())
            {
                case "text":
                case "textarea":
                    attrValue.ValueText = value.ToString();
                    break;
                case "number":
                    if (double.TryParse(value.ToString(), out var num))
                        attrValue.ValueNumber = num;
                    break;
                case "date":
                    if (DateTime.TryParse(value.ToString(), out var date))
                        attrValue.ValueDate = date;
                    break;
                case "boolean":
                    if (bool.TryParse(value.ToString(), out var boolVal))
                        attrValue.ValueBoolean = boolVal;
                    break;
                default:
                    attrValue.ValueText = value.ToString();
                    break;
            }
        }
    }

    public class DashboardStats
    {
        public int TotalObjects { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int OverdueAssignments { get; set; }
        public int InProgressObjects { get; set; }
        public double CompletionRate { get; set; }
    }

    public class XmlImportService
    {
        private readonly ObjectService _objectService;

        public XmlImportService(ObjectService objectService)
        {
            _objectService = objectService;
        }

        public async Task<List<ControlObject>> ImportFromXmlAsync(string xmlContent, int userId)
        {
            var results = new List<ControlObject>();
            var doc = System.Xml.Linq.XDocument.Parse(xmlContent);

            foreach (var element in doc.Descendants("Object"))
            {
                var typeId = int.Parse(element.Attribute("TypeId")?.Value ?? "1");
                var name = element.Element("Name")?.Value ?? "Unnamed";
                var address = element.Element("Address")?.Value ?? "";
                var attrs = new Dictionary<string, object>();

                foreach (var attr in element.Descendants("Attribute"))
                {
                    var attrName = attr.Attribute("Name")?.Value;
                    var value = attr.Value;
                    if (!string.IsNullOrEmpty(attrName))
                    {
                        attrs[attrName] = value;
                    }
                }

                var obj = await _objectService.CreateObjectWithAttributesAsync(
                    typeId, name, address, userId, attrs);
                results.Add(obj);
            }

            return results;
        }
    }
}