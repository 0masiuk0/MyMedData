using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using static System.Math;
using System.Windows.Media.Media3D;

namespace MyMedData.Controls
{
	public class ZoomBorder : Border
	{
		private UIElement child = null;
		private Point origin;
		private Point start;
		

		private TranslateTransform GetTranslateTransform(UIElement element)
		{
			return (TranslateTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is TranslateTransform);
		}

		private ScaleTransform GetScaleTransform(UIElement element)
		{
			return (ScaleTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is ScaleTransform);
		}

		public override UIElement Child
		{
			get { return base.Child; }
			set
			{
				if (value != null && value != this.Child)
					this.Initialize(value);
				base.Child = value;
			}
		}

		public void Initialize(UIElement element)
		{
			this.child = element;
			if (child != null)
			{
				TransformGroup group = new TransformGroup();
				ScaleTransform st = new ScaleTransform();
				group.Children.Add(st);
				TranslateTransform tt = new TranslateTransform();
				group.Children.Add(tt);
				child.RenderTransform = group;
				child.RenderTransformOrigin = new Point(0.0, 0.0);
				this.MouseWheel += child_MouseWheel;
				this.MouseLeftButtonDown += child_MouseLeftButtonDown;
				this.MouseLeftButtonUp += child_MouseLeftButtonUp;				
				this.MouseMove += child_MouseMove;
				this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
				  child_PreviewMouseRightButtonDown);
			}

			BorderThickness = new Thickness(8);
			BorderBrush = new SolidColorBrush(Colors.Red);						
		}
		
		public void Reset()
		{
			if (child != null)
			{
				// reset zoom
				var st = GetScaleTransform(child);
				st.ScaleX = 1.0;
				st.ScaleY = 1.0;

				// reset pan
				var tt = GetTranslateTransform(child);
				tt.X = 0.0;
				tt.Y = 0.0;
			}
		}

		#region Child Events
		private void child_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (child != null)
			{
				var scaleTransform = GetScaleTransform(child);
				var translateTransform = GetTranslateTransform(child);

				double zoom = e.Delta > 0 ? .25 : -.25;
				if (!(e.Delta > 0) && (scaleTransform.ScaleX < .4 || scaleTransform.ScaleY < .4))
					return;

				Point relative = e.GetPosition(child);
				double absoluteX;
				double absoluteY;

				absoluteX = relative.X * scaleTransform.ScaleX + translateTransform.X;
				absoluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y;

				var newScale = Math.Max(1, scaleTransform.ScaleX + zoom);

				scaleTransform.ScaleX = newScale;
				scaleTransform.ScaleY = newScale;

				translateTransform.X = absoluteX - relative.X * scaleTransform.ScaleX;
				translateTransform.Y = absoluteY - relative.Y * scaleTransform.ScaleY;

				ApplyTTboundary(ref translateTransform, scaleTransform);
			}
		}

		private void child_MouseMove(object sender, MouseEventArgs e)
		{
			if (child != null)
			{
				if (child.IsMouseCaptured)
				{
					var translateTransform = GetTranslateTransform(child);
					Vector v = start - e.GetPosition(this);
					translateTransform.X = origin.X - v.X;
					translateTransform.Y = origin.Y - v.Y;
					ApplyTTboundary(ref translateTransform, GetScaleTransform(child));
				}
			}
		}

		private void ApplyTTboundary(ref TranslateTransform tt, ScaleTransform sc)
		{
			if (child is FrameworkElement image)
			{
				double allowedRightTranslation, allowedLeftTranslation, allowedDownTranslation, allowedUpTranslation;
				double scale = sc.ScaleX;
				double horizontalGrowth = image.ActualWidth * (scale - 1);
				double verticalGrowth = image.ActualHeight * (scale - 1);

				allowedRightTranslation = 0;
				allowedLeftTranslation = horizontalGrowth;

				allowedDownTranslation = 0;
				allowedUpTranslation = verticalGrowth;
				
				tt.X = ForceValueToRange(tt.X, -allowedLeftTranslation, allowedRightTranslation);
				tt.Y = ForceValueToRange(tt.Y, -allowedUpTranslation, allowedDownTranslation);
			}	
			
			double ForceValueToRange(double value, double min, double max)
			{
				value = Max(value, min);
				value = Min(value, max);
				return value;
			}
		}

		private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (child != null)
			{
				var tt = GetTranslateTransform(child);
				start = e.GetPosition(this);
				origin = new Point(tt.X, tt.Y);
				this.Cursor = Cursors.Hand;
				child.CaptureMouse();
			}
		}

		private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			StopTranslation();
		}

		private void StopTranslation()
		{
			if (child != null)
			{
				child.ReleaseMouseCapture();
				this.Cursor = Cursors.Arrow;
			}
		}

		void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.Reset();
		}
		
		#endregion

		private T FindParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(child);
			if (parent is T _parent)
				return _parent;
			else
				return FindParent<T>(parent);
		}
	}
}
