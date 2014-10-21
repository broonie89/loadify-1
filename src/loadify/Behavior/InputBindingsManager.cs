using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace loadify.Behavior
{
    /*
     * Thanks to Samuel Jack @http://stackoverflow.com/questions/563195/bind-textbox-on-enter-key-press
     */
    public static class InputBindingsManager
    {

        public static readonly DependencyProperty UpdateSourceWhenEnterPressedProperty = DependencyProperty.RegisterAttached(
            "UpdateSourceWhenEnterPressed", typeof(DependencyProperty), typeof(InputBindingsManager), new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

        static InputBindingsManager()
        { }

        public static void SetUpdateSourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdateSourceWhenEnterPressedProperty, value);
        }

        public static DependencyProperty GetUpdateSourceWhenEnterPressed(DependencyObject dp)
        {
            return (DependencyProperty)dp.GetValue(UpdateSourceWhenEnterPressedProperty);
        }

        private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as UIElement;
            if (element == null) return;

            if (e.OldValue != null)
                element.PreviewKeyDown -= HandlePreviewKeyDown;

            if (e.NewValue != null)
                element.PreviewKeyDown += new KeyEventHandler(HandlePreviewKeyDown);
        }

        static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DoUpdateSource(e.Source);
        }

        static void DoUpdateSource(object source)
        {
            var property = GetUpdateSourceWhenEnterPressed(source as DependencyObject);
            if (property == null) return;

            var elt = source as UIElement;
            if (elt == null) return;

            var binding = BindingOperations.GetBindingExpression(elt, property);
            if (binding == null) return;
            binding.UpdateSource();     
        }
    }
}
