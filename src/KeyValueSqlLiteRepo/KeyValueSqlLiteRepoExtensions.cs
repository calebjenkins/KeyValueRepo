
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Calebs.Data.KeyValueRepo.SqlLite;

public static class KeyValueSqlLiteRepoExtensions
{
    public static KeyValueSqlLiteRepo asKeyValueSqlLiteRepo(this IKeyValueRepo repo)
    {
        if(repo is KeyValueSqlLiteRepo)
        {
            return (KeyValueSqlLiteRepo)repo;
        }
        return null;
    }

    public static void ConfirmOpen(this SqliteConnection db)
    {
        if (db.State != System.Data.ConnectionState.Open)
        {
            db.Open();
        }
    }
}
