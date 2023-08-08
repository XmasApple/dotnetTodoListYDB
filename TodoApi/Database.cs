namespace TodoApi;

using System.Threading.Tasks;
using TodoApi.Models;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

public static class Database
{

    private static TableClient? _tableClient;


    public static async void Init(TableClient tableClient)
    {
        _tableClient = tableClient;

    }
    public static async Task RegisterTables(params TableBase[] tables)
    {
        foreach (var table in tables)
        {
            await ExecuteSchemeQuery(table.GetCreateString());
        }
    }

    public static async Task<ResultSet?> ExecuteDataQuery(
        string queryString,
        Dictionary<string, YdbValue>? parameters = null)
    {
        if (_tableClient is null)
        {
            throw new NullReferenceException($"please call Database.Init()");
        }
        var response = await _tableClient.SessionExec(async session =>
            parameters is null
                ? await session.ExecuteDataQuery(
                    queryString,
                    TxControl.BeginSerializableRW().Commit())
                : (Ydb.Sdk.Client.IResponse)await session.ExecuteDataQuery(
                    queryString,
                    TxControl.BeginSerializableRW().Commit(),
                    parameters));
        response.Status.EnsureSuccess();
        var resultSets = ((ExecuteDataQueryResponse)response).Result.ResultSets;
        if (resultSets.Count == 0) { return null; }
        return ((ExecuteDataQueryResponse)response).Result.ResultSets[0];
    }

    public static async Task ExecuteSchemeQuery(string queryString)
    {
        if (_tableClient is null)
        {
            throw new NullReferenceException("please call Database.Init()");
        }

        var response = await _tableClient.SessionExec(async session =>
             await session.ExecuteSchemeQuery(queryString)
        );
        response.Status.EnsureSuccess();
    }


}