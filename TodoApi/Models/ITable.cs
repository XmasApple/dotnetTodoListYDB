using static Ydb.Sdk.Value.ResultSet;

namespace TodoApi.Models;


public abstract class TableBase
{
    public virtual string GetCreateString()
    {
        throw new NotImplementedException();
    }

    // public ITable FromRow(Row row) { throw new NotImplementedException(); }
    public TableBase() { }
    public TableBase(Row row) { }
}