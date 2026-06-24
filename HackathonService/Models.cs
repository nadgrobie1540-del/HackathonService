using System;
using System.Collections.Generic;

namespace HackathonService
{
    public class BaseEntity
    {
        public int  Id { get; set; }
        public  DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted  { get; set; }
    }
    public class ObjectType : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ControlObject> Objects { get; set; }
        public List<Attribute> Attributes { get; set; }
    }
    public class Attribute : BaseEntity
    {
        public int ObjectTypeId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSearchable { get; set; }
        public string DefaultValue { get; set; }
        public string Options { get; set; }
        public string Section { get; set; }
        public int SortOrder { get; set; }
        public ObjectType ObjectType { get; set; }
        public List<AttributeValue> Values { get; set; }
    }
    public class ControlObject : BaseEntity
    {
        public int ObjectTypeId { get; set; }
        public string Name { get; set; }
        public string Address { get;  set; }
        public string Status { get; set; } = "active";
        public int CreatedBy { get; set; }
        public ObjectType ObjectType { get; set; }
        public List<AttributeValue> AttributeValues { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<Document> Documents { get; set; }
    }



    public class AttributeValue : BaseEntity
    {
        public int ObjectId { get;  set; }
        public int AttributeId { get; set; }
        public string ValueText {   get; set; }
        public double? ValueNumber { get; set; }
        public DateTime? ValueDate { get; set; }
        public  bool? ValueBoolean { get; set; }
        public string ValueJson { get; set; }
        public ControlObject Object { get; set; }
        public Attribute Attribute { get; set; }
    }
    public class Assignment : BaseEntity
    {
        public int ObjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ResponsiblePerson { get; set; }
        public int AssignedBy { get; set; }
        public string Status { get; set; } = "assigned";
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public ControlObject Object { get; set; }
        public List<Document> Documents { get; set; }
    }
    public class Document : BaseEntity
    {
        public  int? ObjectId { get; set; }
        public int? AssignmentId { get; set; }
        public string FileName { get; set; }
        public string FilePath  { get; set; }
        public long FileSize { get;  set; }
        public string MimeType { get; set; }

        public ControlObject Object {  get; set; }
        public Assignment Assignment { get; set; }
    }
}