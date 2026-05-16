#pragma warning disable
using System.Diagnostics;

namespace IntroToTask;

public class Database
{
	static readonly Lazy<Database> _database = new(() => new Database());
	bool _result;

	Database()
	{

	}

	public static Database Current => _database.Value;
}