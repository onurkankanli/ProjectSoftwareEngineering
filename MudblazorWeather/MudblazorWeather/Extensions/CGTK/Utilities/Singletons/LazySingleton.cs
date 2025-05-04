using System;

namespace WeatherClient.Extensions.CGTK.Utilities.Singletons
{
	/// <summary> Lazy Singleton for <see cref="class"/>es.</summary>
	/// <remarks> Lazy = Creates Instance on request. </remarks>
	/// <typeparam name="T"> Type of the Singleton (CRTP). </typeparam>
	public abstract class LazySingleton<T> where T : LazySingleton<T>, new()
	{
		#region Properties

		private static readonly Lazy<T> _internalInstance = new Lazy<T>(valueFactory: () => new T());

		/// <summary> Static reference to the Instance. </summary>
		public static T Instance => _internalInstance.Value;

		/// <summary> Whether there is an instance or not. </summary>
		public static bool InstanceExists => _internalInstance.IsValueCreated;

		#endregion
	}
}