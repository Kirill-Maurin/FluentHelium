namespace FluentHelium.Module {
	/// <summary>
	/// Null-size type, you can use it for argument or return value
	/// </summary>
	public struct Unit
	{
		public static Unit Value { get; } = default;
	}
}