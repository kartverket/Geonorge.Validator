using Serilog.Context;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Geonorge.Validator.Application.Utils
{
    public static class ContextCorrelator
	{
		private static readonly AsyncLocal<ImmutableDictionary<string, object>> _items = new();

		private static ImmutableDictionary<string, object> Items
		{
			get => _items.Value ??= ImmutableDictionary<string, object>.Empty;
			set => _items.Value = value;
		}

		public static object GetValue(string key) => Items[key];

		public static IDisposable BeginCorrelationScope(string key, object value)
		{
			if (Items.ContainsKey(key))
				throw new InvalidOperationException($"{key} is already being correlated!");

			var scope = new CorrelationScope(Items, LogContext.PushProperty(key, value));

			Items = Items.Add(key, value);

			return scope;
		}

		public sealed class CorrelationScope : IDisposable
		{
			private readonly ImmutableDictionary<string, object> _bookmark;
			private readonly IDisposable _logContextPop;

			public CorrelationScope(ImmutableDictionary<string, object> bookmark, IDisposable logContextPop)
			{
				_bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
				_logContextPop = logContextPop ?? throw new ArgumentNullException(nameof(logContextPop));
			}

			public void Dispose()
			{
				_logContextPop.Dispose();
				Items = _bookmark;
			}
		}
	}
}
