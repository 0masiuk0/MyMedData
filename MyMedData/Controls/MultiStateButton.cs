using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MyMedData.Controls
{
	[ContentProperty(nameof(Content))]
	public sealed class MultistateButtonState
	{
		public object Content { get; set; }
		public object ToolTip { get; set; }
		public Style ButtonStyle { get; set; }
		public object ButtonStateTag { get; set; }
	}

	public sealed class MultiButtonStateCollection : Collection<MultistateButtonState>
	{
		private bool _isSealed;

		internal void Seal()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException(
					"MultiStateButton должен содержать хотя бы один MultiButtonState.");
			}

			_isSealed = true;
		}

		private void EnsureWritable()
		{
			if (_isSealed)
			{
				throw new InvalidOperationException(
					"Нельзя менять состав состояний после загрузки MultiStateButton.");
			}
		}

		protected override void InsertItem(int index, MultistateButtonState item)
		{
			EnsureWritable();
			base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));
		}

		protected override void SetItem(int index, MultistateButtonState item)
		{
			EnsureWritable();
			base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));
		}

		protected override void RemoveItem(int index)
		{
			EnsureWritable();
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			EnsureWritable();
			base.ClearItems();
		}
	}

	[ContentProperty(nameof(States))]
	public sealed class MultiStateButton : UserControl
	{
		private readonly Button _button = new Button();
		private int _index;

		public MultiButtonStateCollection States { get; } =
			new MultiButtonStateCollection();

		private static readonly DependencyPropertyKey CurrentStatePropertyKey =
			DependencyProperty.RegisterReadOnly(
				nameof(CurrentState),
				typeof(MultistateButtonState),
				typeof(MultiStateButton),
				new PropertyMetadata(null));

		public static readonly DependencyProperty CurrentStateProperty =
			CurrentStatePropertyKey.DependencyProperty;

		public MultistateButtonState CurrentState =>
			(MultistateButtonState)GetValue(CurrentStateProperty);

		public MultiStateButton()
		{
			Content = _button;

			Loaded += OnFirstLoaded;
			_button.Click += TurnState;
		}

		private void OnFirstLoaded(object sender, RoutedEventArgs e)
		{
			States.Seal();
			ShowState(0);

			// Повторное добавление контрола в визуальное дерево
			// не должно сбрасывать выбор на первое состояние.
			Loaded -= OnFirstLoaded;
		}

		private void TurnState(object sender, RoutedEventArgs e)
		{
			ShowState((_index + 1) % States.Count);
		}

		private void ShowState(int index)
		{
			var state = States[index];

			if (state.ButtonStyle != null)
				_button.Style = state.ButtonStyle;
			else
				_button.ClearValue(FrameworkElement.StyleProperty);

			if (state.ToolTip != null)
				_button.ToolTip = state.ToolTip;
			else
				_button.ClearValue(FrameworkElement.ToolTipProperty);

			_button.Content = state.Content;
			_index = index;
			SetValue(CurrentStatePropertyKey, state);
		}
	}
}