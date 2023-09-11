namespace Maui.Phone;

public class Constants
{
#if IOS
	public static string StoragePath =>
		Foundation.NSFileManager.DefaultManager.GetUrls(
			Foundation.NSSearchPathDirectory.DocumentDirectory, 
			Foundation.NSSearchPathDomain.User)[0]
			.Path;
#else
	public static string StoragePath => Path.Combine(
		Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).Path, 
		"MauiDemo");
#endif
	public const string LogFileName = "DemoLog-.txt";

	public static string LogPath => Path.Combine(StoragePath, LogFileName);
}