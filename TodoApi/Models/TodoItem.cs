using static Ydb.Sdk.Value.ResultSet;

namespace TodoApi.Models;

public class TodoItem : TableBase
{
    public string TodoItemID { get; set; } = "";
    public string? Title { get; set; }
    public string? Description { get; set; }

    public byte? Status { get; set; }

    public TodoItem() {}

    public TodoItem(Row row) : base(row)
    {
        TodoItemID = row["todo_id"].GetUtf8();
        Title = row["title"].GetOptionalUtf8();
        Description = row["description"].GetOptionalUtf8();
        Status = row["status"].GetOptionalUint8();
    }

    public override string GetCreateString()
    {
        return @"
        CREATE TABLE todo_items (
            todo_id UTF8 NOT NULL,
            title UTF8, 
            description UTF8,
            status UINT8,
            PRIMARY KEY (todo_id)
        );";
    }
}
