using Microsoft.AspNetCore.Mvc;

using TodoApi.Models;
using Ydb.Sdk.Value;
using System.Linq;

namespace TodoApi.Controllers;

[ApiController]
[Route("todos")]
public class TodoItemController : ControllerBase
{

    private readonly ILogger<TodoItemController> _logger;

    public TodoItemController(ILogger<TodoItemController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodoById(string id)
    {
        var resultSet = await Database.ExecuteDataQuery(
            @"
            DECLARE $id AS UTF8;
            SELECT * FROM todo_items WHERE todo_id = $id
            ",
            new Dictionary<string, YdbValue> { { "$id", YdbValue.MakeUtf8(id) } }
            );
        if (resultSet?.Rows.Count == 0)
        {
            return NotFound(id);
        }
        var row = resultSet?.Rows[0];
        if (row == null) { return NotFound(id); }
        return Ok(new TodoItem(row));
    }

    [HttpGet()]
    public async Task<IActionResult> GetTodos()
    {
        var resultSet = await Database.ExecuteDataQuery(
            "SELECT * FROM todo_items"
        );
        return Ok(resultSet.Rows.Select(row => new TodoItem(row)).ToList());
    }

    [HttpPost()]
    public async Task<IActionResult> PostTodo(TodoItem todoItem)
    {
        if (todoItem.Title is null or "")
        {
            return BadRequest("Title should be not empty");
        }
        var resultSet = await Database.ExecuteDataQuery(
            @"
            DECLARE $title AS UTF8;
            DECLARE $description AS UTF8;
            DECLARE $status AS UINT8;

            INSERT INTO todo_items (todo_id, title, description, status)
            VALUES (CAST(RandomUuid(1) AS UTF8), $title, $description, $status);
            ",
            new Dictionary<string, YdbValue>{
                { "$title", YdbValue.MakeUtf8(todoItem.Title)},
                { "$description", todoItem.Description is null ? YdbValue.MakeEmptyOptional(YdbTypeId.Utf8): YdbValue.MakeUtf8(todoItem.Description)},
                { "$status", YdbValue.MakeUint8(todoItem.Status??0)}
            }
        );
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodo(string id, TodoItem todoItem)
    {
        if (todoItem.TodoItemID != "" && id != todoItem.TodoItemID)
        {
            return BadRequest("Bad id");
        }

        await Database.ExecuteDataQuery(
            @"
            DECLARE $todo_id AS UTF8;
            DECLARE $title AS UTF8;
            DECLARE $description AS UTF8;
            DECLARE $status AS UINT8;

            UPDATE todo_items
            SET 
                title = IF($title IS NULL, title, $title),
                description = IF($description IS NULL, description, $description),
                status = IF($status IS NULL, status, $status)
            WHERE todo_id = $todo_id
            ",
            new Dictionary<string, YdbValue> {
                { "$todo_id", YdbValue.MakeUtf8(id)},
                { "$title", YdbValue.MakeUtf8(todoItem.Title)},
                { "$description", YdbValue.MakeUtf8(todoItem.Description??"")},
                { "$status", YdbValue.MakeUint8(todoItem.Status??0)},
            }
        );

        return await GetTodoById(id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(string id) {
        var resultSet = await Database.ExecuteDataQuery(
            @"
            DECLARE $id AS UTF8;
            SELECT * FROM todo_items WHERE todo_id = $id
            ",
            new Dictionary<string, YdbValue> { { "$id", YdbValue.MakeUtf8(id) } }
            );
        if (resultSet?.Rows.Count == 0)
        {
            return NotFound(id);
        }
        var row = resultSet?.Rows[0];
        if (row == null) { return NotFound(id); }

        await Database.ExecuteDataQuery(
            @"
            DECLARE $todo_id AS UTF8;

            DELETE FROM todo_items WHERE todo_id = $todo_id;
            ",
            new Dictionary<string, YdbValue> {{"$todo_id", YdbValue.MakeUtf8(id)}}
        );

        return Ok(new TodoItem(row));
    }
}
