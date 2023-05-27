
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Calebs.Data.KeyValueRepo.SqlLite;

public static class KeyValueSqlLiteRepoExtensions
{
    public static KeyValueSqLiteRepo AsKeyValueSqlLiteRepo(this IKeyValueRepo repo)
    {
        if(repo is KeyValueSqLiteRepo)
        {
            return (KeyValueSqLiteRepo)repo;
        }
        throw new ArgumentException("Cannot convert non KeyValueSqlLiteRepo instances.");
    }

    public static void ConfirmOpen(this SqliteConnection db)
    {
        if (db.State != System.Data.ConnectionState.Open)
        {
            db.Open();
        }
    }
}
