using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoCompleteTextBox.Editors;
using System.Windows;

namespace MyMedData.Controls
{
	public class AutocompleteDoctorSuggestionProvider : FrameworkElement, ISuggestionProvider
	{
		public IEnumerable<Doctor> Source
		{
			get => (IEnumerable<Doctor>)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(IEnumerable<Doctor>), typeof(RecordDisplay),
			new PropertyMetadata(null));

		IEnumerable ISuggestionProvider.GetSuggestions(string filter)
		{
			 return Source.Where(d => d.Name.ToLower().StartsWith(filter.ToLower())).ToList();
		}
	}

	public class AutocompleteExaminationTypesSuggestionProvider : FrameworkElement, ISuggestionProvider
	{
		public IEnumerable<ExaminationType> Source
		{
			get => (IEnumerable<ExaminationType>)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(IEnumerable<ExaminationType>), typeof(RecordDisplay),
			new PropertyMetadata(null));

		IEnumerable ISuggestionProvider.GetSuggestions(string filter)
		{
			return Source.Where(d => d.ExaminationTypeTitle.ToLower().StartsWith(filter.ToLower())).ToList();
		}
	}

	public class AutocompleteClinicSuggestionProvider : FrameworkElement, ISuggestionProvider
	{
		public IEnumerable<Clinic> Source
		{
			get => (IEnumerable<Clinic>)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(IEnumerable<Clinic>), typeof(RecordDisplay),
			new PropertyMetadata(null));
		
		IEnumerable ISuggestionProvider.GetSuggestions(string filter)
		{
			return Source.Where(d => d.Name.ToLower().StartsWith(filter.ToLower())).ToList();
		}
	}
}
