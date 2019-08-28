namespace AEServer
{
    public enum AEDBDataType
    {
        ADDT_None       = 0,
        ADDT_INT,
        ADDT_UINT,
        ADDT_FLOAT,
        ADDT_LONG,
        ADDT_ULONG,
        ADDT_DOUBLE,
        ADDT_STRING,
        ADDT_BINARY,
    }

    public interface IDBTableDesc
    {
        string name { get; }

        string keyName { get; }

        int columCount { get; }

        bool initFromConf(object config);

        string getName(int colIndex);

        AEDBDataType getDataType(string colName);
    }

    public interface IDBTable
    {
        ulong dataCount { get; }

        IDBTableDesc desc { get; }

        bool init(object config);

        IDBObject insertData(object Data);

        IDBObject getDataObject(string id);

        bool isExist(string id);

        IDBObject queryDBObject(string query);
        
        object queryData(string query);
    }
}